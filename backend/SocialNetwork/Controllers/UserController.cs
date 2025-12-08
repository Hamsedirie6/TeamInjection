using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AuthService auth, AppDbContext context, IConfiguration config)
        {
            _auth = auth;
            _context = context;
            _config = config;
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var result = _auth.Login(request.Username, request.Password);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            // Hämta user från DB
            var user = _context.Users.First(u => u.Username == request.Username);

            // Skapa JWT token
            var token = GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                Message = result.Message,
                Token = token,
                UserId = user.Id,
                Username = user.Username
            });
        }

        
        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] LoginRequest request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest(new { error = "Username already exists" });

            var user = new User
            {
                Username = request.Username,
                PasswordHash = AuthService.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User created",
                user.Id,
                user.Username
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
                user.Id,
                user.Username
            });
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

       
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
