using System.Text.RegularExpressions;
using viaggia_server.DTOs.Users;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Users;

namespace viaggia_server.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _specificUserRepository;

        public UserService(IRepository<User> userRepository, IUserRepository specificUserRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _specificUserRepository = specificUserRepository ?? throw new ArgumentNullException(nameof(specificUserRepository));
        }

        public async Task<UserDTO> CreateClientAsync(CreateClientDTO request)
        {
            // Validações de negócios
            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format.");
            if (await _specificUserRepository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");
            if (!IsValidCpf(request.Cpf))
                throw new ArgumentException("Invalid CPF format.");
            if (await _specificUserRepository.CpfExistsAsync(request.Cpf))
                throw new ArgumentException("CPF already exists.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Cpf = request.Cpf,
                AddressStreet = request.AddressStreet,
                AddressCity = request.AddressCity,
                AddressState = request.AddressState,
                AddressZipCode = request.AddressZipCode,
                IsActive = true
            };

            var created = await _specificUserRepository.CreateAsync(user, "CLIENT");
            await _userRepository.SaveChangesAsync();
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateServiceProviderAsync(CreateServiceProviderDTO request)
        {
            // Validações de negócios
            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format.");
            if (await _specificUserRepository.EmailExistsAsync(request.Email))
                throw new ArgumentException("Email already exists.");
            if (!IsValidCnpj(request.Cnpj))
                throw new ArgumentException("Invalid CNPJ format.");
            if (await _specificUserRepository.CnpjExistsAsync(request.Cnpj))
                throw new ArgumentException("CNPJ already exists.");

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

            var created = await _specificUserRepository.CreateAsync(user, "SERVICE_PROVIDER");
            await _userRepository.SaveChangesAsync();
            return ToDTO(created);
        }

        public async Task<UserDTO> CreateAttendantAsync(CreateAttendantDTO request)
        {
            // Validações de negócios
            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format.");
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

            var created = await _specificUserRepository.CreateAsync(user, "ATTENDANT");
            await _userRepository.SaveChangesAsync();
            return ToDTO(created);
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(user => ToDTO(user)).ToList();
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new ArgumentException("User not found.");
            return ToDTO(user);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var deleted = await _userRepository.SoftDeleteAsync(id);
            if (!deleted)
                throw new ArgumentException("User not found.");
            await _userRepository.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int id)
        {
            var reactivated = await _specificUserRepository.ReactivateAsync(id);
            if (!reactivated)
                throw new ArgumentException("User not found.");
            await _userRepository.SaveChangesAsync();
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
                AddressStreet = user.AddressStreet,
                AddressCity = user.AddressCity,
                AddressState = user.AddressState,
                AddressZipCode = user.AddressZipCode,
                CompanyName = user.CompanyName,
                Cnpj = user.Cnpj,
                CompanyLegalName = user.CompanyLegalName,
                EmployerCompanyName = user.EmployerCompanyName,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(r => r.Role.Name).ToList()
            };
        }

        private bool IsValidEmail(string email)
        {
            // Validação simples de email com regex
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidCpf(string cpf)
        {
            // Validação de formato de CPF (ex.: 123.456.789-00)
            return Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
        }

        private bool IsValidCnpj(string cnpj)
        {
            // Validação de formato de CNPJ (ex.: 12.345.678/0001-99)
            return Regex.IsMatch(cnpj, @"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$");
        }
    }
}