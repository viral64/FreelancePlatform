namespace FreelancePlatform.Application.Auth;

public interface IAuthService
{
    Task<string>RegisterAsync(string email, string password);
    Task<string> LoginAsync(string email, string password);
}