using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using Socialnetwork.Entityframework;
using SocialNetwork.Entity.Models;

namespace SocialNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly AppDbContext _context;

        public UserController(AuthService auth, AppDbContext context)
        {
            _auth = auth;
            _context = context;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var result = _auth.Login(request.Username, request.Password);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new LoginResponse
            {
                Message = result.Message
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                id = user.Id,
                username = user.Username
            });
        }

        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] LoginRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                PasswordHash = AuthService.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username

                })
                .ToList();

            return Ok(users);
        }


    }
}
