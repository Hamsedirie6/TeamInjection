using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;

namespace SocialNetwork.Test.Services;

public class FollowServiceTests
{
    [Fact]
    public void AddFollow_ShouldFail_WhenSameUser()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new FollowService(context);

        var result = service.AddFollow(new Follow { FollowerId = 1, FollowedId = 1 });

        Assert.False(result.Success);
        Assert.Equal("User cannot follow themselves", result.ErrorMessage);
    }

    [Fact]
    public void AddFollow_ShouldSucceed()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new FollowService(context);

        var result = service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.True(result.Success);
    }

    [Fact]
    public void AddFollow_ShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new FollowService(context);

        service.AddFollow(new Follow { FollowerId = 1, FollowedId = 2 });

        Assert.Equal(1, context.Follows.Count());
    }
}
