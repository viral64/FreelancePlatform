namespace FreelancePlatform.Application.Auth;

public interface IAuthService
{
    Task<string>RegisterAsync(RegisterDto register);
    Task<string> LoginAsync(LoginDto login);
}