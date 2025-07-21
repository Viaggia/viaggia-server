using viaggia_server.DTOs.User;

namespace viaggia_server.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateClientAsync(CreateClientRequest request);
        Task<UserResponse> CreateServiceProviderAsync(CreateServiceProviderRequest request);
        Task<UserResponse> CreateAttendantAsync(CreateAttendantRequest request);
        Task<List<UserResponse>> GetAllAsync();
        Task<UserResponse> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}
