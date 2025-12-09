using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Services;

public class FollowService
{
    private readonly AppDbContext _context;

    public FollowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> AddFollow(Follow follow)
    {
        if (follow.FollowerId == follow.FollowedId)
            return (false, "User cannot follow themselves");

        var followerExists = await _context.Users.AnyAsync(u => u.Id == follow.FollowerId);
        var followedExists = await _context.Users.AnyAsync(u => u.Id == follow.FollowedId);
        if (!followerExists || !followedExists)
            return (false, "User not found");

        _context.Follows.Add(follow);
        _context.SaveChanges();

        return (true, "");
    }

    public IEnumerable<Follow> GetFollowers(int userId)
    {
        return _context.Follows.Where(f => f.FollowedId == userId).ToList();
    }

    public IEnumerable<Follow> GetFollowing(int userId)
    {
        return _context.Follows.Where(f => f.FollowerId == userId).ToList();
    }

    public IEnumerable<int> GetFriends(int userId)
    {
        var followingIds = _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId);

        var followerIds = _context.Follows
            .Where(f => f.FollowedId == userId)
            .Select(f => f.FollowerId);

        return followingIds.Intersect(followerIds).ToList();
    }

    public (bool Success, string ErrorMessage) RemoveFollow(int followerId, int followedId)
    {
        var follow = _context.Follows.FirstOrDefault(f => f.FollowerId == followerId && f.FollowedId == followedId);
        if (follow == null)
            return (false, "Follow relation not found");

        _context.Follows.Remove(follow);
        _context.SaveChanges();

        return (true, "");
    }
}
