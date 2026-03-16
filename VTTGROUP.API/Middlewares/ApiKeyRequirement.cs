using Microsoft.AspNetCore.Authorization;

namespace VTTGROUP.API.Middlewares
{
    public class ApiKeyRequirement : IAuthorizationRequirement { }

    public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public ApiKeyHandler(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            var request = _httpContextAccessor.HttpContext.Request;

            if (!request.Headers.TryGetValue("X-Api-Key", out var keyFromHeader))
                return Task.CompletedTask;

            var validKey = _config["ApiSecurity:ApiKey"];
            if (keyFromHeader == validKey)
            {
                context.Succeed(requirement);
            }
            Console.WriteLine($"[ApiKey] Header: {keyFromHeader}, Valid: {_config["ApiSecurity:ApiKey"]}");
            return Task.CompletedTask;
        }
    }

}
