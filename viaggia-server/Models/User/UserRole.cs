using viaggia_server.Models.Users;

namespace viaggia_server.Models.UserRoles
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}