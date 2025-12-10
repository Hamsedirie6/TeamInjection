using System;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;

namespace SocialNetwork.Test.Services;

public class PostServiceTests
{
    [Fact]
    public async Task CreatePostShouldFailWhenMessageIsEmpty()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePost(new Post { Message = string.Empty }));
    }

    [Fact]
    public async Task CreatePostShouldSucceedWithValidMessage()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var result = await service.CreatePost(new Post { Message = "Hello world" });

        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public async Task CreatePostShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        await service.CreatePost(new Post { Message = "Test" });

        Assert.Equal(1, context.Posts.Count());
    }

    [Fact]
    public async Task CreatePostShouldAssignDateAutomatically()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var post = new Post { Message = "Auto date" };
        await service.CreatePost(post);

        Assert.True(post.CreatedAt != default);
    }

    [Fact]
    public async Task CreatePostShouldFailWhenMessageTooLong()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);
        var longMessage = new string('a', 501);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePost(new Post { Message = longMessage }));
    }
}
