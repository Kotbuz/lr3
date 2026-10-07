namespace UsersProxy.Models
{
    // Пользователь смежной системы (схема User из localhost:8000/swagger)
    public class ExternalUser
    {
        public string? Id { get; set; }
        public string? NewId { get; set; }
        public string? Password { get; set; }
        public string? Surname { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public List<string>? Roles { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CertificateFrom { get; set; }
        public DateTime? CertificateTo { get; set; }
        public bool Archive { get; set; }
        public bool FromAD { get; set; }
        public bool Blocked { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Department { get; set; }
        public string? Organization { get; set; }
        public bool OwnRequests { get; set; }
        public int Notifications { get; set; } = 1;
        public DateTime? CreatedDatetime { get; set; }
        public DateTime? LastUpdateDatetime { get; set; }
        public DateTime? LastPasswordChangeDatetime { get; set; }
        public DateTime? LastLoginDatetime { get; set; }
    }
}
