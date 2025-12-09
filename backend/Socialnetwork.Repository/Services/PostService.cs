using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Services;

public class PostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public (bool Success, string ErrorMessage) CreatePost(Post post)
    {
        if (string.IsNullOrWhiteSpace(post.Message))
            return (false, "Message is required");

        post.CreatedAt = DateTime.UtcNow;

        _context.Posts.Add(post);
        _context.SaveChanges();

        return (true, "");
    }

    public Post? GetById(int id)
    {
        return _context.Posts.FirstOrDefault(p => p.Id == id);
    }

    public IEnumerable<Post> GetByUser(int userId)
    {
        return _context.Posts
            .Where(p => p.FromUserId == userId || p.ToUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public IEnumerable<Post> GetTimeline(int userId)
    {
        var followedIds = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToList();

        // Include own posts in the timeline.
        followedIds.Add(userId);

        return _context.Posts
            .Where(p => followedIds.Contains(p.FromUserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public IEnumerable<Post> GetAll()
    {
        return _context.Posts.ToList();
    }

    public (bool Success, string ErrorMessage) DeletePost(int postId, int userId)
    {
        var post = _context.Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null)
            return (false, "Post not found");

        if (post.FromUserId != userId)
            return (false, "Not allowed to delete this post");

        _context.Posts.Remove(post);
        _context.SaveChanges();

        return (true, "");
    }
}
