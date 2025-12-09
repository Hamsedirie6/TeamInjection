using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

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

            try
            {
                var result = _service.SendMessage(msg);

                // Om du i framtiden vill att service ska kunna returnera andra fel
                if (!result.Success)
                    return BadRequest(new { error = result.ErrorMessage });
            }
            catch (ArgumentException ex)
            {
                // Valideringsfel (t.ex. > 500 tecken)
                return BadRequest(new { error = ex.Message });
            }

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

        [Authorize]
        [HttpGet("conversation/{user1}/{user2}")]
        public IActionResult GetConversation(int user1, int user2)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null)
                return Unauthorized();

            if (currentUserId != user1 && currentUserId != user2)
                return Forbid();

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

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = _service.DeleteMessage(id, userId.Value);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Message not found")
                    return NotFound(new { error = result.ErrorMessage });

                return Forbid();
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("unread")]
        public IActionResult GetUnread([FromQuery] DateTime? since = null)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var messages = _service.GetUnread(userId.Value, since)
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

        [Authorize]
        [HttpGet("threads")]
        public IActionResult GetThreads()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var threads = _service.GetThreads(userId.Value)
                .Select(t => new
                {
                    OtherUserId = t.OtherUserId,
                    LastMessage = new DirectMessageResponse
                    {
                        Id = t.LastMessage.Id,
                        SenderId = t.LastMessage.SenderId,
                        ReceiverId = t.LastMessage.ReceiverId,
                        Message = t.LastMessage.Message,
                        SentAt = t.LastMessage.SentAt
                    }
                });

            return Ok(threads);
        }

        [Authorize]
        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");

            var lastSeen = DateTime.UtcNow;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var newMessages = _service.GetUnread(userId.Value, lastSeen);

                    foreach (var msg in newMessages)
                    {
                        var payload = new DirectMessageResponse
                        {
                            Id = msg.Id,
                            SenderId = msg.SenderId,
                            ReceiverId = msg.ReceiverId,
                            Message = msg.Message,
                            SentAt = msg.SentAt
                        };

                        var json = JsonSerializer.Serialize(payload);
                        await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                        lastSeen = msg.SentAt;
                    }

                    await Response.Body.FlushAsync(cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected or request cancelled.
            }
        }

        private int? GetUserId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (int.TryParse(sub, out var id))
                return id;
            return null;
        }
    }
}
