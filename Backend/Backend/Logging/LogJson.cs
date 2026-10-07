using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Backend.Logging
{
    // Сериализация объектов для логов без экранирования кириллицы
    public static class LogJson
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    }
}
