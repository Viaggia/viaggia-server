using Microsoft.AspNetCore.Authorization;

namespace viaggia_server.Config
{
    public class HotelAccessRequirement : IAuthorizationRequirement
    {
    }

    public class HotelAccessHandler : AuthorizationHandler<HotelAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HotelAccessHandler> _logger;

        public HotelAccessHandler(IHttpContextAccessor httpContextAccessor, ILogger<HotelAccessHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HotelAccessRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext não está disponível na autorização HotelAccess.");
                context.Fail();
                return Task.CompletedTask;
            }

            var hasServiceProviderRole = context.User.IsInRole("SERVICE_PROVIDER");
            var hasHotelIdClaim = context.User.HasClaim(c => c.Type == "HotelId");
            int hotelId = 0; // Initialize hotelId to avoid CS0165 error
            var isValidHotelId = hasHotelIdClaim && int.TryParse(context.User.FindFirst("HotelId")?.Value, out hotelId) && hotelId > 0;

            if (!hasServiceProviderRole || !hasHotelIdClaim || !isValidHotelId)
            {
                _logger.LogWarning(
                    "Falha na autorização para HotelAccess. Role SERVICE_PROVIDER: {HasRole}, HotelId presente: {HasHotelId}, HotelId válido: {IsValidHotelId}",
                    hasServiceProviderRole, hasHotelIdClaim, isValidHotelId);
                context.Fail();
                return Task.CompletedTask;
            }

            _logger.LogInformation("Autorização HotelAccess bem-sucedida para o usuário com HotelId: {HotelId}", hotelId);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
