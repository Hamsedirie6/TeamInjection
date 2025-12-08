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
    public class DirectMessageController : ControllerBase
    {
        private readonly DirectMessageService _service;

        public DirectMessageController(DirectMessageService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpPost]
        public IActionResult Send([FromBody] DirectMessageRequest request)
        {
            var senderId = GetUserId();
            if (senderId == null)
                return Unauthorized();

            var msg = new DirectMessage
            {
                SenderId = senderId.Value,
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

        private int? GetUserId()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
