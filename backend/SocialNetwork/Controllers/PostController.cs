using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace SocialNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly PostService _service;

        public PostController(PostService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] CreatePostRequest request)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var post = new Post
            {
                FromUserId = userId.Value,
                ToUserId = request.ToUserId == 0 ? userId.Value : request.ToUserId,
                Message = request.Message,
            };

            var result = _service.CreatePost(post);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            var response = new PostResponse
            {
                Id = post.Id,
                FromUserId = post.FromUserId,
                ToUserId = post.ToUserId,
                Message = post.Message,
                CreatedAt = post.CreatedAt
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var post = _service.GetById(id);
            if (post == null) return NotFound();

            return Ok(new PostResponse
            {
                Id = post.Id,
                FromUserId = post.FromUserId,
                ToUserId = post.ToUserId,
                Message = post.Message,
                CreatedAt = post.CreatedAt
            });
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUser(int userId)
        {
            var posts = _service.GetByUser(userId)
                .Select(p => new PostResponse
                {
                    Id = p.Id,
                    FromUserId = p.FromUserId,
                    ToUserId = p.ToUserId,
                    Message = p.Message,
                    CreatedAt = p.CreatedAt
                });

            return Ok(posts);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = _service.GetAll()
                .Select(p => new PostResponse
                {
                    Id = p.Id,
                    FromUserId = p.FromUserId,
                    ToUserId = p.ToUserId,
                    Message = p.Message,
                    CreatedAt = p.CreatedAt
                });

            return Ok(posts);
        }

        private int? GetUserId()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
