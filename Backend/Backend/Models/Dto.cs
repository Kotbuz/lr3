namespace Backend.Models
{
    // Модели запросов — повторяют типы из документации фронта

    public class TConnectDialogCola
    {
        public string? Tasty { get; set; }
        public string? Volume { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }

    public class TConnectDialogPolice
    {
        public string? Address { get; set; }
    }

    public class TConnectDialogPizza
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Size { get; set; }
        public string[]? Options { get; set; }
        public string? Thickness { get; set; }
    }

    public class TDigitalList
    {
        public string? Filter { get; set; }
        public string? Sorted { get; set; }
    }

    public class TConnectDialog
    {
        public string[]? RequiredTask { get; set; }
        public string? Phone { get; set; }
        public string? Name { get; set; }
    }

    public class DigitalServiceDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
