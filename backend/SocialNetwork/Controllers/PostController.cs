using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Socialnetwork.Entityframework;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly PostService _service;
        private readonly AppDbContext _context;

        public PostController(PostService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            if (!await _context.Users.AnyAsync(u => u.Id == userId.Value))
                return BadRequest(new { error = "Användaren i token finns inte." });

            if (request.ToUserId != 0 && !await _context.Users.AnyAsync(u => u.Id == request.ToUserId))
                return BadRequest(new { error = "Mottagaranvändaren finns inte." });

            var post = new Post
            {
                FromUserId = userId.Value,
                ToUserId = request.ToUserId == 0 ? userId.Value : request.ToUserId,
                Message = request.Message,
            };

            var result = _service.CreatePost(post);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(MapPost(post));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var post = _service.GetById(id);
            if (post == null) return NotFound();

            return Ok(MapPost(post));
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUser(int userId)
        {
            var posts = _service.GetByUser(userId)
                .Select(MapPost);

            return Ok(posts);
        }

        [Authorize]
        [HttpGet("timeline")]
        public IActionResult GetTimeline()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var posts = _service.GetTimeline(userId.Value)
                .Select(MapPost);

            return Ok(posts);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = _service.GetAll()
                .Select(MapPost);

            return Ok(posts);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _service.DeletePost(id, userId.Value);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Post not found") return NotFound(new { error = result.ErrorMessage });
                return Forbid();
            }

            return NoContent();
        }

        private int? GetUserId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }

        private PostResponse MapPost(Post post)
        {
            var fromUsername = _context.Users.FirstOrDefault(u => u.Id == post.FromUserId)?.Username;
            var toUsername = _context.Users.FirstOrDefault(u => u.Id == post.ToUserId)?.Username;

            return new PostResponse
            {
                Id = post.Id,
                FromUserId = post.FromUserId,
                ToUserId = post.ToUserId,
                FromUsername = fromUsername,
                ToUsername = toUsername,
                Message = post.Message,
                CreatedAt = post.CreatedAt
            };
        }
    }
}
