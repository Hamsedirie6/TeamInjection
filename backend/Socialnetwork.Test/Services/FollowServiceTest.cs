using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Test.Services;

public class FollowServiceTests
{
    [Fact]
    public async Task AddFollow_ShouldFail_WhenSameUser()
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
    public async Task AddFollow_ShouldSucceed()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new FollowService(context);

        var result = await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AddFollow_ShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new FollowService(context);

        await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.Equal(1, context.Follows.Count());
    }

    [Fact]
    public async Task AddFollow_ShouldFail_WhenUserMissing()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new FollowService(context);

        var result = await service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public void GetFriends_ShouldReturnMutualFollowers()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.Follows.AddRange(
            new Follow { FollowerId = 1, FollowedId = 2 },
            new Follow { FollowerId = 2, FollowedId = 1 }
        );
        context.SaveChanges();
        var service = new FollowService(context);

        var friends = service.GetFriends(1).ToList();

        Assert.Single(friends);
        Assert.Equal(2, friends[0]);
    }

    [Fact]
    public void GetFriends_ShouldBeEmpty_WhenNotMutual()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.Follows.Add(new Follow { FollowerId = 1, FollowedId = 2 });
        context.SaveChanges();
        var service = new FollowService(context);

        var friends = service.GetFriends(1).ToList();

        Assert.Empty(friends);
    }

    [Fact]
    public void GetFriends_ShouldHandleMixedRelationsAcrossThreeUsers()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(
            new User { Id = 1, Username = "u1" },
            new User { Id = 2, Username = "u2" },
            new User { Id = 3, Username = "u3" }
        );
        context.Follows.AddRange(
            new Follow { FollowerId = 1, FollowedId = 2 }, // A -> B
            new Follow { FollowerId = 2, FollowedId = 1 }, // B -> A (mutual with A)
            new Follow { FollowerId = 1, FollowedId = 3 }, // A -> C (not mutual)
            new Follow { FollowerId = 3, FollowedId = 2 }  // C -> B (mutual with B, not A)
        );
        context.SaveChanges();
        var service = new FollowService(context);

        var friendsOfA = service.GetFriends(1).ToList();

        Assert.Single(friendsOfA);
        Assert.Equal(2, friendsOfA[0]);
    }
}
