using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowController : ControllerBase
{
    private readonly FollowService _service;

    public FollowController(FollowService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Follow([FromBody] Follow follow)
    {
        _service.AddFollow(follow);
        return Ok(new { message = "Follow added" });
    }
    [HttpGet("followers/{userId}")]
    public IActionResult GetFollowers(int userId)
    {
        return Ok(_service.GetFollowers(userId));
    }

}
