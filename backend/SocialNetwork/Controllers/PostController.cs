using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;

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

        [HttpPost]
        public IActionResult Create([FromBody] CreatePostRequest request)
        {
            // TODO: hämta från JWT senare
            int fromUserId = 1;

            var post = new Post
            {
                FromUserId = fromUserId,
                ToUserId = request.ToUserId,
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
    }
}