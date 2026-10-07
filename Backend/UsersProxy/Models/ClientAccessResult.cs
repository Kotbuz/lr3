namespace UsersProxy.Models
{
    public enum ClientAccessStatus
    {
        Allowed,            // клиента можно обслуживать
        NotServed,          // пользователь архивный или заблокирован
        Blocked,            // заблокирован только что за частые запросы
        InvalidData,        // в заявке нет корректного телефона
        ServiceUnavailable  // смежная система не ответила
    }

    public record ClientAccessResult(ClientAccessStatus Status, string Message, string? UserId = null, bool Created = false)
    {
        public bool IsAllowed => Status == ClientAccessStatus.Allowed;
    }
}
