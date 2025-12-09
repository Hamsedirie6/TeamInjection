using Microsoft.EntityFrameworkCore;
using SocialNetwork.Entity.Models;
using SocialNetwork.Services;
using SocialNetwork.Test.Factories;
using Xunit;

namespace SocialNetwork.Test.Services;

public class FollowServiceTests
{
    [Fact]
    public async Task AddFollowShouldFailWhenSameUser()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" });
        context.SaveChanges();
        var service = new FollowService(context);

        var result = await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 1 });

        Assert.False(result.Success);
        Assert.Equal("User cannot follow themselves", result.ErrorMessage);
    }

    [Fact]
    public async Task AddFollowShouldSucceed()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new FollowService(context);

        var result = await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AddFollowShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new FollowService(context);

        await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.Equal(1, context.Follows.Count());
    }

    [Fact]
    public async Task AddFollowShouldFailWhenUserMissing()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new FollowService(context);

        var result = await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }
}
