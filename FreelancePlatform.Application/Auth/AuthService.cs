using System.Security.Claims;
using FreelancePlatform.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelancePlatform.Application.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

public class AuthService : IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private ViralDbContext _dbContext;

    public AuthService(ViralDbContext _dbContext, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
       this._dbContext = _dbContext;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(string useremail, string password)
    {
        try
        {
            // Check if the email already exists
            var userExists = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == useremail);
            if (userExists != null)
            {
                throw new Exception("Email already exists.");
            }

            // Hash the password
            

            // Create new user
            var user = new User
            {
                Username = useremail
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate JWT token
            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            // Log the error if needed
            throw new Exception($"Registration failed: {ex.Message}");
        }
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == email);
        if (user == null)
        {
            throw new Exception("Invalid email or password.");
        }

        // If passwords are stored as plain text (not recommended), compare directly
        // if (user.Password != password)
        // {
        //     throw new Exception("Invalid email or password.");
        // }

        return GenerateJwtToken(user);
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Username),
            // Add more claims if needed
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}