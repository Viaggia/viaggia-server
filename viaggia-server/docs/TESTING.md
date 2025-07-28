# 🧪 Guia de Testes - Viaggia Server

## 📋 Índice

1. [Configuração do Ambiente de Testes](#-configuração-do-ambiente-de-testes)
2. [Testes Unitários](#-testes-unitários)
3. [Testes de Integração](#-testes-de-integração)
4. [Testes End-to-End](#-testes-end-to-end)
5. [Testes de Performance](#-testes-de-performance)
6. [Testes de Segurança](#-testes-de-segurança)
7. [CI/CD Pipeline](#-cicd-pipeline)

## 🛠 Configuração do Ambiente de Testes

### Pré-requisitos

```bash
# Instalar .NET 8 SDK
dotnet --version

# Instalar SQL Server LocalDB (para testes)
# Instalar Docker (para testes de integração)
```

### Configuração do Projeto de Testes

```bash
# Criar projeto de testes
dotnet new xunit -n Viaggia.Tests
cd Viaggia.Tests

# Adicionar pacotes necessários
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoFixture
dotnet add package Testcontainers.SqlServer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### Estrutura de Diretórios
```
Viaggia.Tests/
├── Unit/
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   └── Validators/
├── Integration/
│   ├── Controllers/
│   ├── Database/
│   └── Services/
├── EndToEnd/
│   ├── UserFlows/
│   ├── ApiFlows/
│   └── Scenarios/
├── Performance/
├── Security/
├── Fixtures/
├── Helpers/
└── TestData/
```

## 🔬 Testes Unitários

### Configuração Base para Testes Unitários

```csharp
// BaseTest.cs
using AutoFixture;
using Moq;
using Microsoft.Extensions.Logging;

namespace Viaggia.Tests.Unit
{
    public abstract class BaseTest
    {
        protected readonly Fixture _fixture;
        protected readonly MockRepository _mockRepository;

        protected BaseTest()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);
            
            // Configurações do AutoFixture
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        protected Mock<ILogger<T>> CreateLoggerMock<T>()
        {
            return new Mock<ILogger<T>>();
        }
    }
}
```

### Testes de Controllers

```csharp
// AuthControllerTests.cs
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Viaggia.Controllers;
using Viaggia.Services.Auth;
using Viaggia.DTOs.Auth;

namespace Viaggia.Tests.Unit.Controllers
{
    public class AuthControllerTests : BaseTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = _mockRepository.Create<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = _fixture.Create<LoginRequest>();
            var expectedResponse = _fixture.Create<LoginResponse>();
            
            _authServiceMock
                .Setup(x => x.LoginAsync(loginRequest))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(expectedResponse);
            
            _authServiceMock.Verify(x => x.LoginAsync(loginRequest), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = _fixture.Create<LoginRequest>();
            
            _authServiceMock
                .Setup(x => x.LoginAsync(loginRequest))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData(null)]
        public async Task Login_InvalidEmail_ReturnsBadRequest(string email)
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = email, Password = "password" };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
```

### Testes de Services

```csharp
// AuthServiceTests.cs
using Microsoft.AspNetCore.Identity;
using Moq;
using FluentAssertions;
using Viaggia.Services.Auth;
using Viaggia.Models.User;
using Viaggia.DTOs.Auth;

namespace Viaggia.Tests.Unit.Services
{
    public class AuthServiceTests : BaseTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _signInManagerMock = CreateSignInManagerMock();
            _jwtServiceMock = _mockRepository.Create<IJwtService>();
            
            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ValidUser_ReturnsLoginResponse()
        {
            // Arrange
            var loginRequest = _fixture.Create<LoginRequest>();
            var user = _fixture.Create<User>();
            var expectedToken = "jwt-token";
            
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);
            
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
                .ReturnsAsync(SignInResult.Success);
            
            _jwtServiceMock
                .Setup(x => x.GenerateTokenAsync(user))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedToken);
            result.User.Email.Should().Be(user.Email);
        }

        private Mock<UserManager<User>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<SignInManager<User>> CreateSignInManagerMock()
        {
            return new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);
        }
    }
}
```

### Testes de Repositories

```csharp
// HotelRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Viaggia.Data;
using Viaggia.Repositories.Hotel;
using Viaggia.Models.Hotel;

namespace Viaggia.Tests.Unit.Repositories
{
    public class HotelRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly HotelRepository _repository;

        public HotelRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new HotelRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingHotel_ReturnsHotel()
        {
            // Arrange
            var hotel = new Hotel
            {
                Name = "Test Hotel",
                Description = "Test Description",
                StarRating = 4,
                IsActive = true
            };
            
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(hotel.HotelId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Hotel");
        }

        [Fact]
        public async Task GetActiveHotelsAsync_ReturnsOnlyActiveHotels()
        {
            // Arrange
            var activeHotel = new Hotel { Name = "Active", IsActive = true };
            var inactiveHotel = new Hotel { Name = "Inactive", IsActive = false };
            
            _context.Hotels.AddRange(activeHotel, inactiveHotel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveHotelsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Active");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

### Testes de Validators

```csharp
// CreateClientDTOValidatorTests.cs
using FluentAssertions;
using FluentValidation.TestHelper;
using Viaggia.DTOs.User;

namespace Viaggia.Tests.Unit.Validators
{
    public class CreateClientDTOValidatorTests
    {
        private readonly CreateClientDTOValidator _validator;

        public CreateClientDTOValidatorTests()
        {
            _validator = new CreateClientDTOValidator();
        }

        [Fact]
        public void Validate_ValidDto_ShouldNotHaveValidationError()
        {
            // Arrange
            var dto = new CreateClientDTO
            {
                Name = "João Silva",
                Email = "joao@example.com",
                Password = "Password123!",
                PhoneNumber = "+5511999999999",
                Cpf = "12345678901",
                DateOfBirth = DateTime.Now.AddYears(-25)
            };

            // Act & Assert
            _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData(null)]
        public void Validate_InvalidName_ShouldHaveValidationError(string name)
        {
            // Arrange
            var dto = new CreateClientDTO { Name = name };

            // Act & Assert
            _validator.TestValidate(dto)
                .ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("")]
        [InlineData("@domain.com")]
        public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
        {
            // Arrange
            var dto = new CreateClientDTO { Email = email };

            // Act & Assert
            _validator.TestValidate(dto)
                .ShouldHaveValidationErrorFor(x => x.Email);
        }
    }
}
```

## 🔗 Testes de Integração

### Configuração Base para Testes de Integração

```csharp
// IntegrationTestBase.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.SqlServer;
using Viaggia.Data;

namespace Viaggia.Tests.Integration
{
    public class IntegrationTestBase : IAsyncLifetime
    {
        private readonly SqlServerContainer _sqlContainer;
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;

        public IntegrationTestBase()
        {
            _sqlContainer = new SqlServerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("YourStrong@Passw0rd")
                .Build();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove o DbContext existente
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        // Adiciona o DbContext com SQL Server de teste
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseSqlServer(_sqlContainer.GetConnectionString());
                        });
                    });
                });

            _client = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _sqlContainer.StartAsync();
            
            // Executar migrations
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _sqlContainer.StopAsync();
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        protected async Task<string> GetJwtTokenAsync()
        {
            var loginData = new
            {
                Email = "admin@viaggia.com",
                Password = "Admin123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return content.Token;
        }
    }
}
```

### Testes de API Endpoints

```csharp
// HotelsControllerIntegrationTests.cs
using System.Net;
using FluentAssertions;
using Viaggia.DTOs.Hotel;

namespace Viaggia.Tests.Integration.Controllers
{
    public class HotelsControllerIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetHotels_ReturnsSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/hotels");

            // Assert
            response.EnsureSuccessStatusCode();
            response.Content.Headers.ContentType?.ToString()
                .Should().Contain("application/json");
        }

        [Fact]
        public async Task CreateHotel_WithValidData_ReturnsCreated()
        {
            // Arrange
            var token = await GetJwtTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var hotelData = new HotelCreationDTO
            {
                Name = "Integration Test Hotel",
                Description = "Test Description",
                StarRating = 4,
                Street = "Test Street, 123",
                City = "Test City",
                State = "TS",
                ZipCode = "12345-678",
                ContactPhone = "+5511999999999",
                ContactEmail = "test@hotel.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels", hotelData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateHotel_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var hotelData = new HotelCreationDTO
            {
                Name = "Test Hotel"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/hotels", hotelData);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetHotel_NonExistentId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/hotels/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
```

### Testes de Database

```csharp
// DatabaseIntegrationTests.cs
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Viaggia.Models.Hotel;
using Viaggia.Models.User;

namespace Viaggia.Tests.Integration.Database
{
    public class DatabaseIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task Database_CanInsertAndRetrieveHotel()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hotel = new Hotel
            {
                Name = "Database Test Hotel",
                Description = "Test Description",
                StarRating = 5,
                IsActive = true,
                CreateDate = DateTime.UtcNow
            };

            // Act
            context.Hotels.Add(hotel);
            await context.SaveChangesAsync();

            var retrievedHotel = await context.Hotels
                .FirstOrDefaultAsync(h => h.Name == "Database Test Hotel");

            // Assert
            retrievedHotel.Should().NotBeNull();
            retrievedHotel.Name.Should().Be("Database Test Hotel");
            retrievedHotel.StarRating.Should().Be(5);
        }

        [Fact]
        public async Task Database_SoftDelete_WorksCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act - Soft delete
            user.IsActive = false;
            user.DeleteDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Assert
            var allUsers = await context.Users.ToListAsync();
            var activeUsers = await context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            allUsers.Should().Contain(u => u.Email == "test@example.com");
            activeUsers.Should().NotContain(u => u.Email == "test@example.com");
        }
    }
}
```

## 🌐 Testes End-to-End

### Configuração com Playwright

```csharp
// E2ETestBase.cs
using Microsoft.Playwright;

namespace Viaggia.Tests.EndToEnd
{
    public class E2ETestBase : IAsyncLifetime
    {
        protected IPlaywright _playwright;
        protected IBrowser _browser;
        protected IBrowserContext _context;
        protected IPage _page;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _page?.CloseAsync();
            await _context?.CloseAsync();
            await _browser?.CloseAsync();
            _playwright?.Dispose();
        }
    }
}
```

### Cenários de Usuário

```csharp
// UserJourneyTests.cs
using FluentAssertions;

namespace Viaggia.Tests.EndToEnd
{
    public class UserJourneyTests : E2ETestBase
    {
        [Fact]
        public async Task UserCanLoginAndBookHotel()
        {
            // Arrange
            await _page.GotoAsync("https://localhost:7000");

            // Act - Login
            await _page.ClickAsync("[data-testid=login-button]");
            await _page.FillAsync("[data-testid=email-input]", "user@example.com");
            await _page.FillAsync("[data-testid=password-input]", "password");
            await _page.ClickAsync("[data-testid=submit-login]");

            // Assert - Login successful
            await _page.WaitForSelectorAsync("[data-testid=user-menu]");

            // Act - Search hotels
            await _page.FillAsync("[data-testid=destination-input]", "Rio de Janeiro");
            await _page.FillAsync("[data-testid=checkin-input]", "2024-12-01");
            await _page.FillAsync("[data-testid=checkout-input]", "2024-12-03");
            await _page.ClickAsync("[data-testid=search-button]");

            // Assert - Hotels displayed
            await _page.WaitForSelectorAsync("[data-testid=hotel-list]");
            var hotelCount = await _page.Locator("[data-testid=hotel-card]").CountAsync();
            hotelCount.Should().BeGreaterThan(0);

            // Act - Book hotel
            await _page.ClickAsync("[data-testid=hotel-card]:first-child [data-testid=book-button]");
            await _page.FillAsync("[data-testid=guest-count]", "2");
            await _page.ClickAsync("[data-testid=confirm-booking]");

            // Assert - Booking confirmed
            await _page.WaitForSelectorAsync("[data-testid=booking-confirmation]");
            var confirmationText = await _page.TextContentAsync("[data-testid=confirmation-message]");
            confirmationText.Should().Contain("successfully");
        }
    }
}
```

## ⚡ Testes de Performance

### Configuração com NBomber

```csharp
// PerformanceTests.cs
using NBomber;
using NBomber.CSharp;

namespace Viaggia.Tests.Performance
{
    public class PerformanceTests
    {
        [Fact]
        public async Task HotelsEndpoint_CanHandle100ConcurrentUsers()
        {
            var scenario = Scenario.Create("hotels_load_test", async context =>
            {
                using var httpClient = new HttpClient();
                
                var response = await httpClient.GetAsync("https://localhost:7000/api/hotels");
                
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .Run();

            // Assert
            var scnStats = stats.AllScenarios[0];
            scnStats.Ok.Count.Should().BeGreaterThan(1000);
            scnStats.Fail.Count.Should().BeLessThan(10);
            scnStats.Ok.Response.Mean.Should().BeLessThan(1000); // < 1 segundo
        }

        [Fact]
        public async Task AuthEndpoint_LoadTest()
        {
            var scenario = Scenario.Create("auth_load_test", async context =>
            {
                using var httpClient = new HttpClient();
                
                var loginData = new
                {
                    Email = $"user{context.ScenarioInfo.ThreadId}@example.com",
                    Password = "Password123!"
                };

                var response = await httpClient.PostAsJsonAsync(
                    "https://localhost:7000/api/auth/login", 
                    loginData);
                
                return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(1))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .Run();

            // Assert performance criteria
            var scnStats = stats.AllScenarios[0];
            scnStats.Ok.Response.Mean.Should().BeLessThan(500); // < 500ms
            scnStats.Ok.Response.StdDev.Should().BeLessThan(200); // Consistência
        }
    }
}
```

## 🔐 Testes de Segurança

### Testes de Autenticação e Autorização

```csharp
// SecurityTests.cs
using System.Net;
using FluentAssertions;

namespace Viaggia.Tests.Security
{
    public class SecurityTests : IntegrationTestBase
    {
        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/user");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AdminEndpoint_WithClientToken_ReturnsForbidden()
        {
            // Arrange
            var clientToken = await GetClientTokenAsync();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

            // Act
            var response = await _client.DeleteAsync("/api/user/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("SELECT * FROM Users")]
        [InlineData("'; DROP TABLE Users; --")]
        [InlineData("<script>alert('xss')</script>")]
        public async Task SearchEndpoint_WithMaliciousInput_ReturnsCleanResponse(string maliciousInput)
        {
            // Act
            var response = await _client.GetAsync($"/api/hotels?search={maliciousInput}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("DROP TABLE");
            content.Should().NotContain("<script>");
        }

        private async Task<string> GetClientTokenAsync()
        {
            var loginData = new
            {
                Email = "client@viaggia.com",
                Password = "Client123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return content.Token;
        }
    }
}
```

### Testes de Rate Limiting

```csharp
// RateLimitingTests.cs
using System.Net;
using FluentAssertions;

namespace Viaggia.Tests.Security
{
    public class RateLimitingTests : IntegrationTestBase
    {
        [Fact]
        public async Task LoginEndpoint_ExceedsRateLimit_ReturnsRetryAfter()
        {
            // Arrange
            var loginData = new
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var tasks = new List<Task<HttpResponseMessage>>();

            // Act - Fazer múltiplas tentativas de login
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_client.PostAsJsonAsync("/api/auth/login", loginData));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            var rateLimitedResponse = responses.FirstOrDefault(r => 
                r.StatusCode == HttpStatusCode.TooManyRequests);
            
            rateLimitedResponse.Should().NotBeNull();
            rateLimitedResponse.Headers.RetryAfter.Should().NotBeNull();
        }
    }
}
```

## 🚀 CI/CD Pipeline

### GitHub Actions Workflow

```yaml
# .github/workflows/tests.yml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: YourStrong@Passw0rd
          ACCEPT_EULA: Y
        ports:
          - 1433:1433
        options: --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q 'SELECT 1'" --health-interval=10s --health-timeout=5s --health-retries=3

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Run Unit Tests
      run: dotnet test Viaggia.Tests/Unit --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Run Integration Tests
      run: dotnet test Viaggia.Tests/Integration --no-build --verbosity normal
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost;Database=ViaggiaTestDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
        
    - name: Run Security Tests
      run: dotnet test Viaggia.Tests/Security --no-build --verbosity normal
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
        
    - name: Run Performance Tests
      run: dotnet test Viaggia.Tests/Performance --no-build --verbosity normal
      
  e2e-tests:
    runs-on: ubuntu-latest
    needs: test
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Install Playwright
      run: |
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install
        
    - name: Start Application
      run: |
        dotnet run --project Viaggia.Server &
        sleep 30
        
    - name: Run E2E Tests
      run: dotnet test Viaggia.Tests/EndToEnd --no-build --verbosity normal
```

### Test Configuration

```json
// appsettings.Testing.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ViaggiaTestDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "Key": "test-jwt-key-for-testing-purposes-only",
    "Issuer": "ViaggiaTestApi",
    "Audience": "ViaggiaTestApp",
    "DurationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  }
}
```

### Comandos de Teste

```bash
# Executar todos os testes
dotnet test

# Executar apenas testes unitários
dotnet test --filter Category=Unit

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "Name~Login"

# Executar testes em paralelo
dotnet test --parallel

# Gerar relatório de cobertura
reportgenerator -reports:"coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### Métricas de Qualidade

- **Cobertura de Código**: Mínimo 80%
- **Testes Unitários**: > 90% dos métodos públicos
- **Testes de Integração**: Todos os endpoints principais
- **Performance**: Response time < 1s (95th percentile)
- **Security**: Sem vulnerabilidades críticas ou altas

---

**📝 Nota**: Adapte os testes conforme a evolução do projeto e mantenha a documentação atualizada.
