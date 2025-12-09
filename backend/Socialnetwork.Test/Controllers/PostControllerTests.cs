using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Controllers;
using SocialNetwork.DTO;
using SocialNetwork.Entity.Models;
using SocialNetwork.Services;
using SocialNetwork.Test.Factories;
using System.Security.Claims;
using Xunit;

namespace SocialNetwork.Test.Controllers;

public class PostControllerTests
{
    private static ClaimsPrincipal BuildUser(int userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenUserInTokenMissing()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);
        var controller = new PostController(service, context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = BuildUser(99) }
            }
        };

        var result = await controller.Create(new CreatePostRequest { Message = "Hello" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Användaren i token finns inte", badRequest.Value?.ToString());
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenToUserMissing()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.Add(new User { Id = 1, Username = "sender" });
        context.SaveChanges();
        var service = new PostService(context);
        var controller = new PostController(service, context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = BuildUser(1) }
            }
        };

        var result = await controller.Create(new CreatePostRequest { Message = "Hello", ToUserId = 2 });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Mottagaranvändaren finns inte", badRequest.Value?.ToString());
    }
}
