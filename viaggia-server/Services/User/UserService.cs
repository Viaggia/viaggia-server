using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Users;
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

        public UserService(
            IRepository<User> userRepository,
            IUserRepository specificUserRepository,
            AppDbContext context,
            ILogger<UserService> logger,
            IValidator<CreateClientDTO> clientValidator,
            IValidator<CreateServiceProviderDTO> serviceProviderValidator,
            IValidator<CreateAttendantDTO> attendantValidator)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _specificUserRepository = specificUserRepository ?? throw new ArgumentNullException(nameof(specificUserRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
            _serviceProviderValidator = serviceProviderValidator ?? throw new ArgumentNullException(nameof(serviceProviderValidator));
            _attendantValidator = attendantValidator ?? throw new ArgumentNullException(nameof(attendantValidator));
        }

        public async Task<UserDTO> CreateClientAsync(CreateClientDTO request)
        {
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
                Cnpj = request.Cnpj,
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
            _logger.LogInformation("Creating admin for email: {Email}", request.Email);

            // Simples validação manual (ou use um Validator se quiser)
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


        public async Task<List<UserDTO>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all active users");
            var users = await _userRepository.GetAllAsync();
            return users.Select(ToDTO).ToList();
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving user with ID: {Id}", id);
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("User not found.");
            return ToDTO(user);
        }

        public async Task SoftDeleteAsync(int id)
        {
            _logger.LogInformation("Soft deleting user with ID: {Id}", id);
            var deleted = await _userRepository.SoftDeleteAsync(id);
            if (!deleted)
                throw new ArgumentException("User not found.");
            await _userRepository.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int id)
        {
            _logger.LogInformation("Reactivating user with ID: {Id}", id);
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new ArgumentException("User not found.");

            user.IsActive = true;
            _context.Users.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        private async Task<User> CreateUserWithRoleAsync(User user, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                ?? throw new ArgumentException($"Role {roleName} not found.");

            var created = await _userRepository.AddAsync(user);
            await _context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
            await _userRepository.SaveChangesAsync();
            return created;
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
                Cnpj = user.Cnpj,
                CompanyLegalName = user.CompanyLegalName,
                EmployerCompanyName = user.EmployerCompanyName,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
        }
    }
}