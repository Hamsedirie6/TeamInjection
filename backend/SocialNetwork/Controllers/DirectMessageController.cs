using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DirectMessageController : ControllerBase
{
    private readonly DirectMessageService _service;

    public DirectMessageController(DirectMessageService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Send([FromBody] DirectMessage message)
    {
        _service.SendMessage(message);
        return Ok(new { message = "Message sent" });
    }
    [HttpGet("conversation/{user1}/{user2}")]
    public IActionResult GetConversation(int user1, int user2)
    {
        return Ok(_service.GetConversation(user1, user2));
    }

}
