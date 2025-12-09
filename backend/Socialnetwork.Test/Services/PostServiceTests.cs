using System;
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
}
