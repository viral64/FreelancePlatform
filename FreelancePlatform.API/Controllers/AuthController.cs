using FreelancePlatform.Application.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FreelancePlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string email,string password)
        {
            try
            {
                var token = await _authService.RegisterAsync(email,  password);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email,string password)
        {
            try
            {
                var token = await _authService.LoginAsync(email, password);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
