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

    public IEnumerable<Post> GetAll()
    {
        return _context.Posts.ToList();
    }
}
