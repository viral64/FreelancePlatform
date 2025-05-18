using System.Security.Claims;
using FreelancePlatform.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelancePlatform.Application.Auth;

using FreelancePlatform.Application.Helpers;
using FreelancePlatform.Domain.Entities;
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

    public async Task<string> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var userExists = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (userExists != null)
            {
                throw new Exception("Email already exists.");
            }

            var hashedPassword = PasswordHasher.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                Username=registerDto.userName,
                Email = registerDto.Email,
                Password = hashedPassword
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(); // Get UserId

            // Add UserTypeMapping (1 = Client, 2 = Freelancer)
            var userTypeMapping = new UserTypeMapping
            {
                UserId = user.Id,
                UserTypeId = registerDto.IsClient ? 1 : 2,
                AssignedAt = DateTime.UtcNow
            };
            _dbContext.UserTypeMappings.Add(userTypeMapping);

            // Add UserRole (e.g., RoleId = 1 for normal users)
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 2,
                AssignedAt = DateTime.UtcNow
            };
            _dbContext.UserRoles.Add(userRole);

            await _dbContext.SaveChangesAsync();

            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            throw new Exception($"Registration failed: {ex.Message}");
        }
    }


    public async Task<string> LoginAsync(LoginDto login)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
        if (user == null || !PasswordHasher.VerifyPassword(login.Password, user.Password))
        {
            throw new Exception("Invalid email or password.");
        }

        return GenerateJwtToken(user);
    }


    private string GenerateJwtToken(User user)
    {
        var userRole = _dbContext.UserRoles
       .Include(ur => ur.Role)
       .FirstOrDefault(ur => ur.UserId == user.Id);
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Username)
    };
        if (userRole != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name)); // Adds "Admin" or "User"
        }

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
    public string userName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsClient { get; set; }
}
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}