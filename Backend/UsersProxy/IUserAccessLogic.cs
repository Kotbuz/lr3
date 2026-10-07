using UsersProxy.Models;

namespace UsersProxy
{
    // Логика работы с клиентами лендинга через смежную систему учёта пользователей
    public interface IUserAccessLogic
    {
        // Находит клиента по телефону (или создаёт личный кабинет), проверяет, что он не архивный и не заблокирован,
        // и блокирует его за слишком частые заявки
        Task<ClientAccessResult> CheckClientAsync(string? name, string? phone, CancellationToken ct = default);
    }
}
