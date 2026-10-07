namespace Backend.Models
{
    public class ColaOrder
    {
        public int Id { get; set; }
        public string? Tasty { get; set; }
        public string? Volume { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PizzaOrder
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Size { get; set; }
        public string[] Options { get; set; } = [];
        public string? Thickness { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DigitalService
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class Feedback
    {
        public int Id { get; set; }
        public string[] RequiredTask { get; set; } = [];
        public string? Phone { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
