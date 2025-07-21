namespace viaggia_server.Models.User
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public string? CompanyName { get; set; } // Only for ServiceProvider
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
