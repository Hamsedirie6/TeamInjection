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

        [HttpGet("following/{userId}")]
        public IActionResult GetFollowing(int userId)
        {
            var list = _service.GetFollowing(userId)
                .Select(f => new FollowResponse
                {
                    Id = f.Id,
                    FollowerId = f.FollowerId,
                    FollowedId = f.FollowedId
                });

            return Ok(list);
        }

        [Authorize]
        [HttpGet("friends")]
        public IActionResult GetFriends()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var friendIds = _service.GetFriends(userId.Value);
            return Ok(friendIds);
        }

        [Authorize]
        [HttpDelete("{followedId}")]
        public IActionResult Unfollow(int followedId)
        {
            var followerId = GetUserId();
            if (followerId == null)
                return Unauthorized();

            var result = _service.RemoveFollow(followerId.Value, followedId);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Follow relation not found") return NotFound(new { error = result.ErrorMessage });
                return BadRequest(new { error = result.ErrorMessage });
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
    }
}
