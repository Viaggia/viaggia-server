using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Users;
using viaggia_server.Repositories;
using viaggia_server.Repositories.Users;
using viaggia_server.Services.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger
builder.Services.AddSwaggerGen(); // Swagger

try
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
} catch (Exception ex)
{
    Console.WriteLine("Erro ao configurar o DbContext: " + ex.Message);
}
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Configurar autenticação (assumindo JWT ou Identity)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Configurar opções de JWT posteriormente
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme= CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    var config = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // Caminho de retorno após autenticação
    options.ClaimActions.MapJsonKey("picture", "picture", "url");
    options.SaveTokens = true;

    options.Scope.Add("email");
    options.Scope.Add("profile");

    options.Events.OnCreatingTicket = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var googleId = context.Principal.FindFirst("sub")?.Value;
        var email = context.Principal.FindFirst("email")?.Value;
        var name = context.Principal.FindFirst("name")?.Value;
        var avatar = context.Principal.FindFirst("picture")?.Value;

        var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);
        if (user == null)
        {
            user = new User
            {
                GoogleId = googleId,
                Email = email,
                Name = name,
                AvatarUrl = avatar,
                IsActive = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Para servir imagens no wwwroot
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
