using viaggia_server.Models.UserRoles;

namespace viaggia_server.Models.Users
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ADMIN, CLIENTE, ATENDENTE

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
