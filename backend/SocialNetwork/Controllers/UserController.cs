using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Services;
using SocialNetwork.DTOs.Users;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AuthService _auth;

    public UserController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _auth.Login(request.Username, request.Password);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new LoginResponse
        {
            Message = result.Message
        });
    }
}
