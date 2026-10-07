using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UsersProxy.Models;

namespace UsersProxy
{
    public class UserAccessLogic : IUserAccessLogic
    {
        public const string NotServedMessage = "Вы не обслуживаетесь";

        private readonly IUsersApiClient _api;
        private readonly RequestAttemptTracker _tracker;
        private readonly UsersProxyOptions _options;
        private readonly ILogger<UserAccessLogic> _logger;

        public UserAccessLogic(IUsersApiClient api, RequestAttemptTracker tracker,
            IOptions<UsersProxyOptions> options, ILogger<UserAccessLogic> logger)
        {
            _api = api;
            _tracker = tracker;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ClientAccessResult> CheckClientAsync(string? name, string? phone, CancellationToken ct = default)
        {
            // Логин пользователя в смежной системе — нормализованный номер телефона
            var userId = NormalizePhone(phone);
            if (userId == null)
            {
                _logger.LogWarning("Заявка без корректного телефона: {Phone}", phone);
                return new ClientAccessResult(ClientAccessStatus.InvalidData, "Укажите корректный номер телефона");
            }

            // Объясните, точно тут не будет утечки пвмяти, может лучше использовать rabbitMQ?
            var attempts = _tracker.Register(userId, TimeSpan.FromSeconds(_options.WindowSeconds));

            try
            {
                var created = false;
                var user = await _api.GetUserAsync(userId, ct);
                if (user == null)
                {
                    user = await CreateClientAsync(userId, name, ct);
                    created = true;
                }

                if (user.Archive || user.Blocked)
                {
                    _logger.LogWarning("Пользователь {UserId} не обслуживается: archive = {Archive}, blocked = {Blocked}",
                        userId, user.Archive, user.Blocked);
                    return new ClientAccessResult(ClientAccessStatus.NotServed, NotServedMessage, userId);
                }

                if (attempts > _options.MaxAttempts)
                {
                    await BlockAsync(user, ct);
                    _logger.LogWarning("Пользователь {UserId} заблокирован: {Attempts} заявок за {Window} с",
                        userId, attempts, _options.WindowSeconds);
                    return new ClientAccessResult(ClientAccessStatus.Blocked,
                        $"{NotServedMessage}: слишком много заявок подряд, пользователь заблокирован", userId);
                }

                _logger.LogInformation("Пользователь {UserId} допущен (заявка {Attempts} из {Max} за {Window} с){Created}",
                    userId, attempts, _options.MaxAttempts, _options.WindowSeconds, created ? ", личный кабинет создан" : "");
                return new ClientAccessResult(ClientAccessStatus.Allowed, "OK", userId, created);
            }
            catch (UsersApiException e)
            {
                _logger.LogError(e, "Ошибка сервиса пользователей при проверке {UserId}", userId);
                return new ClientAccessResult(ClientAccessStatus.ServiceUnavailable,
                    "Ошибка сервиса пользователей, попробуйте позже", userId);
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(e, "Сервис пользователей недоступен при проверке {UserId}", userId);
                return new ClientAccessResult(ClientAccessStatus.ServiceUnavailable,
                    "Сервис пользователей недоступен, попробуйте позже", userId);
            }
        }

        private async Task<ExternalUser> CreateClientAsync(string userId, string? name, CancellationToken ct)
        {
            await EnsureClientRoleAsync(ct);

            var (surname, firstName, middleName) = SplitName(name);

            // Добавить паттерн Builder и FluentApi
            var user = new ExternalUser
            {
                Id = userId,
                NewId = userId,
                Password = GeneratePassword(),
                Surname = surname,
                FirstName = firstName,
                MiddleName = middleName,
                Roles = [_options.ClientRole],
                Email = $"{userId}@{_options.EmailDomain}",
                PhoneNumber = userId,
                Department = _options.Department,
                Organization = _options.Organization
            };

            try
            {
                var created = await _api.CreateUserAsync(user, ct);
                _logger.LogInformation("В системе пользователей создан личный кабинет {UserId} ({Surname} {FirstName})",
                    userId, surname, firstName);
                return created;
            }
            catch (UsersApiException e) when (e.StatusCode == HttpStatusCode.InternalServerError)
            {
                // Параллельная заявка могла создать пользователя раньше нас
                return await _api.GetUserAsync(userId, ct) ?? throw e;
            }
        }

        // Блокировка через метод update (POST /users/false): archive = true, blocked = true
        private async Task BlockAsync(ExternalUser user, CancellationToken ct)
        {
            user.NewId = user.Id;
            // Update требует пароль, а GET его не отдаёт — заблокированному пользователю выдаём новый случайный
            user.Password = GeneratePassword();
            user.Archive = true;
            user.Blocked = true;
            // Сервис не возвращает эти поля, но требует их при сохранении
            user.MiddleName = string.IsNullOrWhiteSpace(user.MiddleName) ? "-" : user.MiddleName;
            user.Email ??= $"{user.Id}@{_options.EmailDomain}";
            user.Department ??= _options.Department;
            user.Organization ??= _options.Organization;

            await _api.UpdateUserAsync(user, ct);
            _tracker.Reset(user.Id!);
        }

        private async Task EnsureClientRoleAsync(CancellationToken ct)
        {
            var roles = await _api.GetRolesAsync(ct);
            if (roles.Contains(_options.ClientRole, StringComparer.OrdinalIgnoreCase)) return;

            await _api.CreateRoleAsync(_options.ClientRole, ct);
            _logger.LogInformation("В системе пользователей создана роль {Role}", _options.ClientRole);
        }

        // "8 (999) 000-11-22" и "+7 999 000 11 22" → "79990001122"
        public static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits[0] == '8') digits = "7" + digits[1..];
            return digits.Length is >= 5 and <= 15 ? digits : null;
        }

        // "Иванов Иван Иванович" → фамилия, имя, отчество; одно слово считаем именем
        private static (string Surname, string FirstName, string MiddleName) SplitName(string? name)
        {
            var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch
            {
                0 => ("Клиент", "Без имени", "-"),
                1 => ("Клиент", parts[0], "-"),
                2 => (parts[0], parts[1], "-"),
                _ => (parts[0], parts[1], string.Join(' ', parts[2..]))
            };
        }

        // Пароль клиенту не сообщается: кабинет создаётся для демо-стенда, требования сервиса — от 10 символов
        // Добавить возможность выдать временный просто пароль пользователю, можно использовать номер телефона для генерации
        private static string GeneratePassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%^&*";
            const string all = upper + lower + digits + symbols;

            var chars = new List<char>
            {
                upper[RandomNumberGenerator.GetInt32(upper.Length)],
                lower[RandomNumberGenerator.GetInt32(lower.Length)],
                digits[RandomNumberGenerator.GetInt32(digits.Length)],
                symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
            };
            while (chars.Count < 16)
                chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);

            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
    }
}
