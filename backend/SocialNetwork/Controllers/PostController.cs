using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;

namespace SocialNetwork.Controllers;

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
    public IActionResult Create([FromBody] Post post)
    {
        var result = _service.CreatePost(post);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(new { message = "Post created" });
    }
}
