using FreelancePlatform.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
        private ViralDbContext _dbContext;

        public UserController(ViralDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // GET
        [HttpGet("/index")]
        public IActionResult Index()
        {
           var list= _dbContext.Users.ToList();
           return Ok(list);
        }
        
    }
}
