using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Auth;
using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;
using viaggia_server.Services.Media;

namespace viaggia_server.Repositories.Users
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IImageService _imageService;

        public UserRepository(AppDbContext context, IImageService imageService) : base(context)
        {
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CpfExistsAsync(string? cpf)
        {
            return cpf != null && await _context.Users.AnyAsync(u => u.Cpf == cpf);
        }

        public async Task<UserDTO> CreateClientAsync(CreateClientDTO request)
        {
            if (await EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");
            if (await CpfExistsAsync(request.Cpf))
                throw new ArgumentException("CPF already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Cpf = request.Cpf,
                IsActive = true
            };

            var created = await CreateWithRoleAsync(user, "CLIENT");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request)
        {
            if (await EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");

            var user = new User
            {
                Name = request.ResponsibleName,
                CompanyName = request.CompanyName,
                CompanyLegalName = request.CompanyLegalName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true
            };

            var created = await CreateWithRoleAsync(user, "SERVICE_PROVIDER");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request)
        {
            if (await EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");

            var user = new User
            {
                Name = request.Name,
                EmployerCompanyName = request.EmployerCompanyName,
                EmployeeId = request.EmployeeId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true
            };

            var created = await CreateWithRoleAsync(user, "ATTENDANT");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateAdminAsync(CreateAdminDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and Password are required.");

            if (await EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true
            };

            var created = await CreateWithRoleAsync(user, "ADMIN");
            return ToDTO(created);
        }

        public async Task<User> CreateWithRoleAsync(User user, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                throw new ArgumentException($"Role '{roleName}' does not exist.");
            }

            var createdUser = await AddAsync(user);
            await _context.UserRoles.AddAsync(new UserRole
            {
                UserId = createdUser.Id,
                RoleId = role.Id
            });
            await SaveChangesAsync();
            return createdUser;
        }

        public async Task<User> CreateOrLoginOAuth(OAuthRequest dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleUid);
            if (user == null)
            {
                var generatedPassword = Guid.NewGuid().ToString();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

                user = new User
                {
                    GoogleId = dto.GoogleUid,
                    Email = dto.Email,
                    Name = dto.Name,
                    AvatarUrl = dto.Picture,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty,
                    Password = hashedPassword,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow
                };
                user = await CreateWithRoleAsync(user, "CLIENT");
            }
            else
            {
                user.Email = dto.Email;
                user.Name = dto.Name;
                user.AvatarUrl = dto.Picture;
                await UpdateAsync(user);
            }
            return user;
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var users = await base.GetAllAsync();
            return users.Select(ToDTO).ToList();
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await base.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            return ToDTO(user);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            return await base.SoftDeleteAsync(id);
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return false;
            }
            user.IsActive = true;
            await UpdateAsync(user);
            return true;
        }

        public async Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request)
        {
            var user = await base.GetByIdAsync(id)
                ?? throw new ArgumentException("User not found.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            if (roles.Contains("CLIENT") && !string.IsNullOrEmpty(request.Cpf))
            {
                if (request.Cpf != user.Cpf && await CpfExistsAsync(request.Cpf))
                    throw new ArgumentException("CPF already exists.");
            }
            else if (!roles.Contains("CLIENT") && !string.IsNullOrEmpty(request.Cpf))
            {
                throw new ArgumentException("CPF can only be updated for CLIENT role.");
            }

            if (roles.Contains("SERVICE_PROVIDER"))
            {
                if (string.IsNullOrEmpty(request.CompanyName) || string.IsNullOrEmpty(request.CompanyLegalName))
                    throw new ArgumentException("CompanyName and CompanyLegalName are required for SERVICE_PROVIDER.");
            }
            else if (!string.IsNullOrEmpty(request.CompanyName) || !string.IsNullOrEmpty(request.CompanyLegalName))
            {
                throw new ArgumentException("CompanyName and CompanyLegalName can only be updated for SERVICE_PROVIDER role.");
            }

            if (roles.Contains("ATTENDANT"))
            {
                if (string.IsNullOrEmpty(request.EmployerCompanyName) || string.IsNullOrEmpty(request.EmployeeId))
                    throw new ArgumentException("EmployerCompanyName and EmployeeId are required for ATTENDANT.");
            }
            else if (!string.IsNullOrEmpty(request.EmployerCompanyName) || !string.IsNullOrEmpty(request.EmployeeId))
            {
                throw new ArgumentException("EmployerCompanyName and EmployeeId can only be updated for ATTENDANT role.");
            }

            user.Name = request.Name;
            user.PhoneNumber = request.PhoneNumber;
            user.Cpf = request.Cpf;
            user.CompanyName = request.CompanyName;
            user.CompanyLegalName = request.CompanyLegalName;
            user.EmployerCompanyName = request.EmployerCompanyName;
            user.EmployeeId = request.EmployeeId;

            if (request.Avatar != null)
            {
                user.AvatarUrl = await _imageService.UploadImageAsync(request.Avatar, id.ToString())
                    ?? throw new Exception("Failed to upload avatar image.");
            }

            await base.UpdateAsync(user);
            return ToDTO(user);
        }

        private UserDTO ToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Cpf = user.Cpf,
                CompanyName = user.CompanyName,
                CompanyLegalName = user.CompanyLegalName,
                EmployerCompanyName = user.EmployerCompanyName,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }
    }
}