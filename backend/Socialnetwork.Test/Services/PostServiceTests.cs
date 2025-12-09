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
        var post = new Post { FromUserId = 1, ToUserId = 1, Message = "" };

        var ex = Assert.Throws<ArgumentException>(() => service.CreatePost(post));

        Assert.Equal("Message is required", ex.Message);
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
    public void CreatePost_ShouldThrow_WhenMessageIsEmpty()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);
        var post = new Post
        {
            FromUserId = 1,
            ToUserId = 2,
            Message = " "
        };

        // act + assert
        var ex = Assert.Throws<ArgumentException>(() => service.CreatePost(post));
        Assert.Equal("Message is required", ex.Message);
    }

    [Fact]
    public void CreatePost_ShouldFail_WhenMessageTooLong()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);
        var longText = new string('a', 501);

        var post = new Post { FromUserId = 1, ToUserId = 1, Message = longText };

        var ex = Assert.Throws<ArgumentException>(() => service.CreatePost(post));

        Assert.Equal("Message cannot exceed 500 characters", ex.Message);
    }
    [Fact]
    public void CreatePost_ShouldSucceed_WhenMessageIsValid()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var post = new Post
        {
            FromUserId = 1,
            ToUserId = 2,
            Message = "Hej, detta är ett giltigt inlägg."
        };

        // act
        var result = service.CreatePost(post);

        // assert
        Assert.True(result.Success);
        Assert.Equal("", result.ErrorMessage);
        Assert.Single(context.Posts);
        Assert.NotEqual(default, post.CreatedAt);
    }
}
