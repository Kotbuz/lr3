namespace funny.Models
{
    public class ConnectDialogPolice
    {
        public long Id { get; set; }
        public string? UserAgent { get; set; }
        public string? Status { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
