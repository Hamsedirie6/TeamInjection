using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SocialNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly FollowService _service;

        public FollowController(FollowService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpPost]
        public IActionResult Follow([FromBody] FollowRequest request)
        {
            var followerId = GetUserId();
            if (followerId == null)
                return Unauthorized();

            var follow = new Follow
            {
                FollowerId = followerId.Value,
                FollowedId = request.FollowedId
            };

            var result = _service.AddFollow(follow);

            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new FollowResponse
            {
                Id = follow.Id,
                FollowerId = follow.FollowerId,
                FollowedId = follow.FollowedId
            });
        }

        [HttpGet("followers/{userId}")]
        public IActionResult GetFollowers(int userId)
        {
            var list = _service.GetFollowers(userId)
                .Select(f => new FollowResponse
                {
                    Id = f.Id,
                    FollowerId = f.FollowerId,
                    FollowedId = f.FollowedId
                });

            return Ok(list);
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
