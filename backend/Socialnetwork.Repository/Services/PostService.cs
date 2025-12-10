using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Services;

public class PostService
{
    private readonly AppDbContext context;

    public PostService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> CreatePost(Post post)
    {
        if (string.IsNullOrWhiteSpace(post.Message))
            throw new ArgumentException("Message is required", nameof(post.Message));

        if (post.Message.Length > 500)
            throw new ArgumentException("Message cannot exceed 500 characters", nameof(post.Message));

        post.CreatedAt = DateTime.UtcNow;

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public Post? GetById(int id)
    {
        return context.Posts.FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Post> GetByUser(int userId)
    {
        return context.Posts
            .Where(p => p.FromUserId == userId || p.ToUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public IEnumerable<Post> GetTimeline(int userId)
    {
        var followedIds = context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToList();

        // Include own posts in the timeline.
        followedIds.Add(userId);

        return context.Posts
            .Where(p => followedIds.Contains(p.FromUserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public IEnumerable<Post> GetAll()
    {
        return context.Posts.ToList();
    }

    public async Task<(bool Success, string ErrorMessage)> DeletePost(int postId, int userId)
    {
        var post = context.Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null)
            return (false, "Post not found");

        if (post.FromUserId != userId)
            return (false, "Not allowed to delete this post");

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }
}
