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

        _context.Posts.Add(post);
        _context.SaveChanges();

        return (true, "");
    }
}
