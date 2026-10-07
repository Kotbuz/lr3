using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UsersProxy.Models;

namespace UsersProxy
{
    public class UsersApiClient : IUsersApiClient
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly ILogger<UsersApiClient> _logger;

        public UsersApiClient(HttpClient http, ILogger<UsersApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ExternalUser?> GetUserAsync(string id, CancellationToken ct = default)
        {
            using var response = await _http.GetAsync($"users/{Uri.EscapeDataString(id)}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            await EnsureSuccessAsync(response, "GET /users/{id}", ct);
            return await response.Content.ReadFromJsonAsync<ExternalUser>(Json, ct);
        }

        public Task<ExternalUser> CreateUserAsync(ExternalUser user, CancellationToken ct = default) =>
            SaveUserAsync(user, isNewUser: true, ct);

        public Task<ExternalUser> UpdateUserAsync(ExternalUser user, CancellationToken ct = default) =>
            SaveUserAsync(user, isNewUser: false, ct);

        public async Task<List<string>> GetRolesAsync(CancellationToken ct = default)
        {
            using var response = await _http.GetAsync("roles", ct);
            await EnsureSuccessAsync(response, "GET /roles", ct);
            return await response.Content.ReadFromJsonAsync<List<string>>(Json, ct) ?? [];
        }

        public async Task CreateRoleAsync(string name, CancellationToken ct = default)
        {
            // API принимает название роли JSON-строкой: "client"
            using var response = await _http.PostAsJsonAsync("roles", name, Json, ct);
            await EnsureSuccessAsync(response, "POST /roles", ct);
        }

        private async Task<ExternalUser> SaveUserAsync(ExternalUser user, bool isNewUser, CancellationToken ct)
        {
            var action = isNewUser ? "POST /users/true" : "POST /users/false";
            using var response = await _http.PostAsJsonAsync($"users/{(isNewUser ? "true" : "false")}", user, Json, ct);
            await EnsureSuccessAsync(response, action, ct);
            return await response.Content.ReadFromJsonAsync<ExternalUser>(Json, ct)
                   ?? throw new UsersApiException($"{action}: пустой ответ сервиса пользователей", response.StatusCode);
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, string action, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Сервис пользователей: {Action} вернул {Status}: {Body}", action, (int)response.StatusCode, body);
            throw new UsersApiException($"{action} вернул {(int)response.StatusCode}: {body}", response.StatusCode);
        }
    }

    public class UsersApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public UsersApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
