namespace VTTGROUP.Blazor.Services.Auth
{
    public interface IAuthorizationService
    {
        Task<bool> IsAuthenticatedAsync();
    }
}
