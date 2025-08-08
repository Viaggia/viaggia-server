using viaggia_server.DTOs.Auth;
using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.Users;

namespace viaggia_server.Repositories.Users
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CpfExistsAsync(string? cpf);
        Task<User> CreateWithRoleAsync(User user, string roleName);
        Task<UserDTO> CreateClientAsync(CreateClientDTO request);
        Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request);
        Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request);
        Task<UserDTO> CreateAdminAsync(CreateAdminDTO request);
        Task<User> CreateOrLoginOAuth(OAuthRequest dto);
        Task<UserDTO> GetByIdAsync(int id);
        Task<List<UserDTO>> GetAllAsync(); 
        Task<bool> ReactivateAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request);
    }
}