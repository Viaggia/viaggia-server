namespace viaggia_server.Models.User
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ADMIN, CLIENTE, etc

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
