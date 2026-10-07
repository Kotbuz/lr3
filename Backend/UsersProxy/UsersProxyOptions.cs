namespace UsersProxy
{
    // Настройки подключения к смежной системе учёта пользователей (секция "UsersProxy" в appsettings.json)
    public class UsersProxyOptions
    {
        public const string SectionName = "UsersProxy";

        // Адрес сервиса пользователей (TrainingManagerSecurity)
        public string BaseUrl { get; set; } = "http://localhost:8000/";

        // Роль, которая выдаётся клиентам с лендинга
        public string ClientRole { get; set; } = "client";

        // Значения обязательных полей, которых нет в заявке с фронта
        public string Department { get; set; } = "Клиенты";
        public string Organization { get; set; } = "LandingPage";
        public string EmailDomain { get; set; } = "client.local";

        // Защита от частых запросов: больше MaxAttempts заявок за WindowSeconds секунд — блокировка
        public int MaxAttempts { get; set; } = 3;
        public int WindowSeconds { get; set; } = 60;

        public int TimeoutSeconds { get; set; } = 10;
    }
}
