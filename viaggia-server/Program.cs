using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Viaggia.Swagger;
using viaggia_server.Config;
using viaggia_server.Data;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Auth;
using viaggia_server.Repositories.CommodityRepository;
using viaggia_server.Repositories.HotelRepository;
using viaggia_server.Repositories.ReserveRepository;
using viaggia_server.Repositories.Users;
using viaggia_server.Services;
using viaggia_server.Services.Email;
using viaggia_server.Services.HotelServices;
using viaggia_server.Services.ImageService;
using viaggia_server.Services.Payment;
using viaggia_server.Services.ReservationServices;
using viaggia_server.Services.Reserves;
using viaggia_server.Swagger;
using viaggia_server.Validators;

var builder = WebApplication.CreateBuilder(args);

// Read environment variables
builder.Configuration.AddEnvironmentVariables();

// Make DI validation explicit during Build (surfacing the exact failing service)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Toggle: allow enabling Swagger in Production for troubleshooting
var enableSwagger = builder.Environment.IsDevelopment() ||
                    builder.Configuration.GetValue<bool>("Development:EnableSwagger");

// ==============================
// Validate required configuration (fail-fast with clear messages)
// ==============================
string? dbConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(dbConn))
{
    throw new InvalidOperationException(
        "Missing ConnectionStrings:DefaultConnection. " +
        "In Azure App Service, set an App Setting named 'ConnectionStrings__DefaultConnection' with the full SQL connection string.");
}

string? jwtKeyRaw = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKeyRaw))
{
    throw new InvalidOperationException(
        "Missing Jwt:Key. Set 'Jwt__Key' in App Settings (plain text or Base64-encoded).");
}

string? jwtIssuer = builder.Configuration["Jwt:Issuer"];
string? jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Missing Jwt:Issuer and/or Jwt:Audience. Set 'Jwt__Issuer' and 'Jwt__Audience' in App Settings.");
}

// Presence-only boot diagnostics (no secrets)
Console.WriteLine($"[BOOT] Has ConnectionStrings:DefaultConnection? {!string.IsNullOrWhiteSpace(dbConn)}");
Console.WriteLine($"[BOOT] Has Jwt:Key? {!string.IsNullOrWhiteSpace(jwtKeyRaw)}");
Console.WriteLine($"[BOOT] Has Jwt:Issuer? {!string.IsNullOrWhiteSpace(jwtIssuer)}");
Console.WriteLine($"[BOOT] Has Jwt:Audience? {!string.IsNullOrWhiteSpace(jwtAudience)}");

// ==============================
// Health checks
// ==============================
builder.Services.AddHealthChecks();

// ==============================
// Controllers + JSON options
// ==============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    });

// ==============================
// Swagger (with security)
// ==============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Viaggia Server API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    c.EnableAnnotations();
    c.SchemaFilter<EnumSchemaFilter>();
    c.SchemaFilter<FormFileSchemaFilter>();
    c.OperationFilter<SecurityRequirementsOperationFilter>();
    c.OperationFilter<MultipartFormDataOperationFilter>();
});

// ==============================
// DbContext (SQL Server)
// ==============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        dbConn,
        sqlOptions => sqlOptions.CommandTimeout(60)
    )
);

// ==============================
// Repositories / Services
// ==============================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IHotelServices, HotelServices>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IReserveRepository, ReserveRepository>();
builder.Services.AddScoped<IReservesService, ReservesService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
builder.Services.AddScoped<ICommodityRepository, CommodityRepository>();
builder.Services.AddScoped<ICustomCommodityRepository, CustomCommodityRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IGoogleAccountRepository, GoogleAccountRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<Stripe.TokenService>();
builder.Services.AddScoped<Stripe.CustomerService>();
builder.Services.AddScoped<Stripe.ChargeService>();
builder.Services.AddScoped<Stripe.PaymentIntentService>();
builder.Services.AddScoped<Stripe.ProductService>();

// ==============================
// Stripe (reads Stripe__SecretKey from App Settings)
// ==============================
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// ==============================
// FluentValidation
// ==============================
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientDTOValidator>();
builder.Services.AddFluentValidationClientsideAdapters();

// ==============================
// Logging
// ==============================
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddHttpContextAccessor();

// ==============================
// Authentication: JWT + Cookie + optional Google
// ==============================

// Accept Base64 or plain text for JWT key
byte[] jwtKeyBytes;
try
{
    jwtKeyBytes = Convert.FromBase64String(jwtKeyRaw!);
}
catch
{
    jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKeyRaw!);
}

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

authBuilder.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var authRepository = context.HttpContext.RequestServices.GetRequiredService<IAuthRepository>();
            var token = context.SecurityToken as JwtSecurityToken;
            if (token != null && await authRepository.IsTokenRevokedAsync(token.RawData))
            {
                context.Fail("Token foi revogado.");
            }
        }
    };
});

authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
});

// Google (only if both ClientId and ClientSecret exist)
var googleId = builder.Configuration["Authentication:Google:ClientId"];
var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleId!;
        options.ClientSecret = googleSecret!;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = true;
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey("picture", "picture", "url");
    });
}

// ==============================
// CORS (supports array section or CSV via CORS:AllowedOrigins)
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var originsSection = builder.Configuration.GetSection("CORS:AllowedOrigins");
        string[] origins = Array.Empty<string>();
        if (originsSection.Exists())
        {
            origins = originsSection.Get<string[]>() ?? Array.Empty<string>();
        }

        if (origins.Length == 0)
        {
            var originsCsv = builder.Configuration["CORS:AllowedOrigins"];
            origins = (originsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            // DEV-only fallback. In Production, always specify explicit origins.
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ==============================
// Upload limits
// ==============================
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
});

// ==============================
// Build app (with diagnostic on failure)
// ==============================
WebApplication app;
try
{
    app = builder.Build();
}
catch (Exception ex)
{
    Console.WriteLine("FATAL: Host build failed:");
    Console.WriteLine(ex.ToString());
    throw;
}

// ==============================
// Middleware pipeline
// ==============================

// Forwarded headers BEFORE HTTPS redirection
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto |
                       ForwardedHeaders.XForwardedHost,
    // In dynamic proxy environments, avoid strict symmetry
    RequireHeaderSymmetry = false
});

// Swagger (Dev or if explicitly enabled via Development__EnableSwagger=true)
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Viaggia Server API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
