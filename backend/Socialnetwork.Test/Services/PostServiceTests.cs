using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Socialnetwork.Entityframework;
using SocialNetwork.Services;

public class PostServiceTests
{
    [Fact]
    public void CreatePost_ShouldSavePost_ToDatabase()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new PostService(context);

        var post = new Post
        {
            FromUserId = 1,
            ToUserId = 2,
            Message = "Hello world"
        };

        // act
        var result = service.CreatePost(post);

        // assert
        Assert.True(result.Success);
        Assert.Single(context.Posts);
        Assert.Equal("Hello world", context.Posts.First().Message);
    }
}
