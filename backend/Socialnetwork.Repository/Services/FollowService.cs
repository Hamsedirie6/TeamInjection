using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Services;

public class FollowService
{
    private readonly AppDbContext _context;

    public FollowService(AppDbContext context)
    {
        _context = context;
    }

    public (bool Success, string ErrorMessage) AddFollow(Follow follow)
    {
        if (follow.FollowerId == follow.FollowedId)
            return (false, "User cannot follow themselves");

        _context.Follows.Add(follow);
        _context.SaveChanges();

        return (true, "");
    }

    public IEnumerable<Follow> GetFollowers(int userId)
    {
        return _context.Follows.Where(f => f.FollowedId == userId).ToList();
    }
}
