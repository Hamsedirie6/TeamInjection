using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTOs.DirectMessages;
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
    public IActionResult Send([FromBody] DirectMessageRequest request)
    {
        int senderId = 1; // TODO: JWT senare

        var msg = new DirectMessage
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Message = request.Message
        };

        var result = _service.SendMessage(msg);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var response = new DirectMessageResponse
        {
            Id = msg.Id,
            SenderId = msg.SenderId,
            ReceiverId = msg.ReceiverId,
            Message = msg.Message,
            SentAt = msg.SentAt
        };

        return Ok(response);
    }

    [HttpGet("conversation/{user1}/{user2}")]
    public IActionResult GetConversation(int user1, int user2)
    {
        var messages = _service.GetConversation(user1, user2)
            .Select(m => new DirectMessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Message = m.Message,
                SentAt = m.SentAt
            });

        return Ok(messages);
    }
}
