using System;
using System.Linq;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;

namespace SocialNetwork.Test.Services;

public class PostServiceTests
{
    [Fact]
    public void CreatePost_ShouldFail_WhenMessageIsEmpty()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        Assert.Throws<ArgumentException>(() => service.CreatePost(new Post { Message = "" }));
    }

    [Fact]
    public void CreatePost_ShouldSucceed_WithValidMessage()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var result = service.CreatePost(new Post { Message = "Hello world" });

        Assert.True(result.Success);
        Assert.Equal("", result.ErrorMessage);
    }

    [Fact]
    public void CreatePost_ShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        service.CreatePost(new Post { Message = "Test" });

        Assert.Equal(1, context.Posts.Count());
    }

    [Fact]
    public void CreatePost_ShouldAssignDateAutomatically()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var post = new Post { Message = "Auto date" };
        service.CreatePost(post);

        Assert.True(post.CreatedAt != default);
    }

    [Fact]
    public void CreatePost_ShouldFail_WhenMessageTooLong()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);
        var longMessage = new string('a', 501);

        Assert.Throws<ArgumentException>(() => service.CreatePost(new Post { Message = longMessage }));
    }

    [Fact]
    public void GetTimeline_ShouldReturnOwnAndFollowedPosts_SortedDescending()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        context.Follows.Add(new Follow { FollowerId = 1, FollowedId = 2 });

        context.Posts.AddRange(
            new Post { FromUserId = 2, ToUserId = 1, Message = "B older", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new Post { FromUserId = 2, ToUserId = 1, Message = "B newer", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Post { FromUserId = 1, ToUserId = 1, Message = "A own", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
        );
        context.SaveChanges();

        var timeline = service.GetTimeline(1).ToList();

        Assert.Equal(3, timeline.Count);
        Assert.Equal(new[] { "A own", "B newer", "B older" }, timeline.Select(p => p.Message).ToArray());
    }

    [Fact]
    public void GetTimeline_ShouldReturnOnlyOwnPosts_WhenNoFollows()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        context.Posts.AddRange(
            new Post { FromUserId = 1, ToUserId = 1, Message = "A post 1", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new Post { FromUserId = 1, ToUserId = 1, Message = "A post 2", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new Post { FromUserId = 2, ToUserId = 1, Message = "B post", CreatedAt = DateTime.UtcNow.AddMinutes(-3) }
        );
        context.SaveChanges();

        var timeline = service.GetTimeline(1).ToList();

        Assert.Equal(2, timeline.Count);
        Assert.All(timeline, p => Assert.Equal(1, p.FromUserId));
    }

    [Fact]
    public void GetTimeline_ShouldHandleFollowedUserWithNoPosts()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        context.Follows.Add(new Follow { FollowerId = 1, FollowedId = 2 });
        context.Posts.Add(new Post
        {
            FromUserId = 1,
            ToUserId = 1,
            Message = "A only",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        });
        context.SaveChanges();

        var timeline = service.GetTimeline(1).ToList();

        Assert.Single(timeline);
        Assert.Equal(1, timeline[0].FromUserId);
        Assert.Equal("A only", timeline[0].Message);
    }
}
