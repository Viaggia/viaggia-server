using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;

namespace viaggia_server.Services.Users
{
    public interface IUserService
    {
        Task<UserDTO> CreateClientAsync(CreateClientDTO request);
        Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request);
        Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request);
        Task<UserDTO> CreateAdminAsync(CreateAdminDTO request);
        Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request); // New method
    }

}