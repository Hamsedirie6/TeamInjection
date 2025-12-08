using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;

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

        [HttpPost]
        public IActionResult Follow([FromBody] FollowRequest request)
        {
            int followerId = 1; // TODO: byt till JWT

            var follow = new Follow
            {
                FollowerId = followerId,
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
    }
}