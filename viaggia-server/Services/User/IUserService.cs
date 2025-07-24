using viaggia_server.DTOs.Users;

namespace viaggia_server.Services.Users
{
    public interface IUserService
    {
        Task<UserDTO> CreateClientAsync(CreateClientDTO request);
        Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request);
        Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request);
        Task<UserDTO> CreateAdminAsync(CreateAdminDTO request);

        Task<List<UserDTO>> GetAllAsync();
        Task<UserDTO> GetByIdAsync(int id);
        Task SoftDeleteAsync(int id);
        Task ReactivateAsync(int id);
    }
}