using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Viaggia.Swagger;
using viaggia_server.Config;
using viaggia_server.Data;
using viaggia_server.Repositories;
using viaggia_server.Repositories.ReservationRepository;
using viaggia_server.Repositories.Users;
using viaggia_server.Repositories.Auth;
using viaggia_server.Repositories.Commodities;
using viaggia_server.Repositories.HotelRepository;
<<<<<<< HEAD
using viaggia_server.Services;
using viaggia_server.Services.Payment;
=======
using viaggia_server.Repositories.Payment;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.EmailResetPassword;
using viaggia_server.Services.HotelServices;
using viaggia_server.Services.Medias;
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
using viaggia_server.Swagger;
using viaggia_server.Validators;
using viaggia_server.Services;
using viaggia_server.Services.Media;
using viaggia_server.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    });

// Add Swagger
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
        new string[] { }
    }
    });
    c.EnableAnnotations();
    c.SchemaFilter<EnumSchemaFilter>();
    c.SchemaFilter<FormFileSchemaFilter>();
    c.OperationFilter<SecurityRequirementsOperationFilter>();
    c.OperationFilter<MultipartFormDataOperationFilter>();
});

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.CommandTimeout(60)));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<ICommoditieRepository, CommoditieRepository>();
builder.Services.AddScoped<ICommoditieServicesRepository, CommoditieServicesRepository>();
<<<<<<< HEAD
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
=======
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IGoogleAccountRepository, GoogleAccountRepository>();

//Services
builder.Services.AddScoped<IHotelServices, HotelServices>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<Stripe.TokenService>();
builder.Services.AddScoped<Stripe.CustomerService>();
builder.Services.AddScoped<Stripe.ChargeService>();
builder.Services.AddScoped<Stripe.PaymentIntentService>();
builder.Services.AddScoped<Stripe.ProductService>();

// Configure Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientDTOValidator>();
builder.Services.AddFluentValidationClientsideAdapters();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Configure authentication (JWT and Google OAuth)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SaveTokens = true;
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey("picture", "picture", "url");
    options.Events.OnCreatingTicket = context =>
    {
        foreach (var claim in context.Principal!.Claims)
        {
            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
<<<<<<< HEAD
        policy.WithOrigins(
                "http://localhost:5173",
                "https://your-production-frontend.com"
            ).AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
=======
        //policy.WithOrigins(
        //        "http://localhost:5173",
        //        "https://your-production-frontend.com"
        //    )
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();

>>>>>>> 4ab8ac3dc4732ca91d9c662fc8b90e047b46890d
    });
});

// Add file upload support
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Viaggia Server API v1"));
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseStaticFiles(); // For serving images in wwwroot
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
