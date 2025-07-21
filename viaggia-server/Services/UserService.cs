using viaggia_server.DTOs.User;
using viaggia_server.Models.User;
using viaggia_server.Repositories.Interfaces;
using viaggia_server.Services.Interfaces;

namespace viaggia_server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserResponse> CreateClientAsync(CreateClientRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            var created = await _repository.CreateAsync(user, "CLIENT");
            return ToResponse(created);
        }

        public async Task<UserResponse> CreateServiceProviderAsync(CreateServiceProviderRequest request)
        {
            var user = new User
            {
                Name = request.ResponsibleName,
                CompanyName = request.CompanyName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            var created = await _repository.CreateAsync(user, "SERVICE_PROVIDER");
            return ToResponse(created);
        }

        public async Task<UserResponse> CreateAttendantAsync(CreateAttendantRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            var created = await _repository.CreateAsync(user, "ATTENDANT");
            return ToResponse(created);
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(user => ToResponse(user)).ToList();
        }

        public async Task<UserResponse> GetByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id)
                ?? throw new Exception("User not found.");
            return ToResponse(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id)
                ?? throw new Exception("User not found.");
            await _repository.DeleteAsync(user);
        }

        private UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyName = user.CompanyName,
                Roles = user.UserRoles.Select(r => r.Role.Name).ToList()
            };
        }
    }
}
