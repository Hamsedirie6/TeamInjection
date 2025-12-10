using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Services;

public class FollowService
{
    private readonly AppDbContext context;

    public FollowService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> AddFollow(Follow follow)
    {
        if (follow.FollowerId == follow.FollowedId)
            return (false, "User cannot follow themselves");

        var followerExists = await context.Users.AnyAsync(u => u.Id == follow.FollowerId);
        var followedExists = await context.Users.AnyAsync(u => u.Id == follow.FollowedId);
        if (!followerExists || !followedExists)
            return (false, "User not found");

        context.Follows.Add(follow);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public IEnumerable<Follow> GetFollowers(int userId)
    {
        return context.Follows.Where(f => f.FollowedId == userId).ToList();
    }

    public IEnumerable<Follow> GetFollowing(int userId)
    {
        return context.Follows.Where(f => f.FollowerId == userId).ToList();
    }

    public IEnumerable<int> GetFriends(int userId)
    {
        var followingIds = context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId);

        var followerIds = context.Follows
            .Where(f => f.FollowedId == userId)
            .Select(f => f.FollowerId);

        return followingIds.Intersect(followerIds).ToList();
    }

    public (bool Success, string ErrorMessage) RemoveFollow(int followerId, int followedId)
    {
        var follow = context.Follows.FirstOrDefault(f => f.FollowerId == followerId && f.FollowedId == followedId);
        if (follow == null)
            return (false, "Follow relation not found");

        context.Follows.Remove(follow);
        context.SaveChanges();

        return (true, string.Empty);
    }
}
