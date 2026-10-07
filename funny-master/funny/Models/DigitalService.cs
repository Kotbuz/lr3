
namespace funny.Models
{
    public class DigitalService
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

namespace funny.Models
{
    public class DigitalListRequest
    {
        public string? Filter { get; set; }
        public string? Sorted { get; set; } 
    }
}
