using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.User;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Media;
using viaggia_server.Validators;
using ValidationException = FluentValidation.ValidationException;

namespace viaggia_server.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _specificUserRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IValidator<CreateClientDTO> _clientValidator;
        private readonly IValidator<CreateServiceProviderDTO> _serviceProviderValidator;
        private readonly IValidator<CreateAttendantDTO> _attendantValidator;
        private readonly IValidator<UpdateUserDTO> _updateUserValidator; // New validator
        private readonly IImageService _imageService; // New service

        public UserService(
            IRepository<User> userRepository,
            IUserRepository specificUserRepository,
            AppDbContext context,
            ILogger<UserService> logger,
            IValidator<CreateClientDTO> clientValidator,
            IValidator<CreateServiceProviderDTO> serviceProviderValidator,
            IValidator<CreateAttendantDTO> attendantValidator,
            IValidator<UpdateUserDTO> updateUserValidator,
            IImageService imageService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _specificUserRepository = specificUserRepository ?? throw new ArgumentNullException(nameof(specificUserRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
            _serviceProviderValidator = serviceProviderValidator ?? throw new ArgumentNullException(nameof(serviceProviderValidator));
            _attendantValidator = attendantValidator ?? throw new ArgumentNullException(nameof(attendantValidator));
            _updateUserValidator = updateUserValidator ?? throw new ArgumentNullException(nameof(updateUserValidator));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        public async Task<UserDTO> CreateClientAsync(CreateClientDTO request)
        {
            // Existing implementation (unchanged)
            var validator = new CreateClientDTOValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await _specificUserRepository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");
            if (await _specificUserRepository.CpfExistsAsync(request.Cpf))
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

            var created = await CreateUserWithRoleAsync(user, "CLIENT");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request)
        {
            // Existing implementation (unchanged)
            _logger.LogInformation("Creating service provider for email: {Email}", request.Email);
            var validationResult = await _serviceProviderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await _specificUserRepository.EmailExistsAsync(request.Email))
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

            var created = await CreateUserWithRoleAsync(user, "SERVICE_PROVIDER");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request)
        {
            // Existing implementation (unchanged)
            _logger.LogInformation("Creating attendant for email: {Email}", request.Email);
            var validationResult = await _attendantValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await _specificUserRepository.EmailExistsAsync(request.Email))
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

            var created = await CreateUserWithRoleAsync(user, "ATTENDANT");
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateAdminAsync(CreateAdminDTO request)
        {
            // Existing implementation (unchanged)
            _logger.LogInformation("Creating admin for email: {Email}", request.Email);
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and Password are required.");

            if (await _specificUserRepository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true
            };

            var created = await CreateUserWithRoleAsync(user, "ADMIN");
            return ToDTO(created);
        }
        public async Task<UserDTO> UpdateAsync(int id, UpdateUserDTO request)
        {
            _logger.LogInformation("Updating user with ID: {Id}", id);

            // Validate request
            var validationResult = await _updateUserValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // Get user
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("User not found.");

            // Check role-specific field constraints
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            if (roles.Contains("CLIENT") && !string.IsNullOrEmpty(request.Cpf))
            {
                if (request.Cpf != user.Cpf && await _specificUserRepository.CpfExistsAsync(request.Cpf))
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

            // Update fields
            user.Name = request.Name;
            user.PhoneNumber = request.PhoneNumber;
            user.Cpf = request.Cpf;
            user.CompanyName = request.CompanyName;
            user.CompanyLegalName = request.CompanyLegalName;
            user.EmployerCompanyName = request.EmployerCompanyName;
            user.EmployeeId = request.EmployeeId;

            // Handle image upload
            if (request.Avatar != null)
            {
                user.AvatarUrl = await _imageService.UploadImageAsync(request.Avatar, id.ToString())
                    ?? throw new Exception("Failed to upload avatar image.");
            }

            // Update user using generic repository
            await _userRepository.UpdateAsync(user);

            return ToDTO(user);
        }

        private async Task<User> CreateUserWithRoleAsync(User user, string roleName)
        {
            // Existing implementation (unchanged)
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                ?? throw new ArgumentException($"Role {roleName} not found.");

            var created = await _userRepository.AddAsync(user);
            await _context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
            await _userRepository.SaveChangesAsync();
            return created;
        }

        private UserDTO ToDTO(User user)
        {
            // Existing implementation (unchanged)
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