using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Services;

public class DirectMessageService
{
    private readonly AppDbContext _context;

    public DirectMessageService(AppDbContext context)
    {
        _context = context;
    }

    public (bool Success, string ErrorMessage) SendMessage(DirectMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Message))
            return (false, "Message cannot be empty");

        message.SentAt = DateTime.UtcNow;

        _context.DirectMessages.Add(message);
        _context.SaveChanges();

        return (true, "");
    }

    public IEnumerable<DirectMessage> GetConversation(int user1, int user2)
    {
        return _context.DirectMessages
            .Where(m =>
                (m.SenderId == user1 && m.ReceiverId == user2) ||
                (m.SenderId == user2 && m.ReceiverId == user1))
            .OrderBy(m => m.SentAt)
            .ToList();
    }

    public IEnumerable<DirectMessage> GetUnread(int userId, DateTime? sinceUtc = null, int limit = 100)
    {
        var since = sinceUtc ?? DateTime.UtcNow.AddMinutes(-30);

        return _context.DirectMessages
            .Where(m => m.ReceiverId == userId && m.SentAt >= since)
            .OrderBy(m => m.SentAt)
            .Take(limit)
            .ToList();
    }

    public IEnumerable<(int OtherUserId, DirectMessage LastMessage)> GetThreads(int userId)
    {
        var relevantMessages = _context.DirectMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToList();

        return relevantMessages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var last = g.OrderByDescending(m => m.SentAt).First();
                var otherUserId = last.SenderId == userId ? last.ReceiverId : last.SenderId;
                return (OtherUserId: otherUserId, LastMessage: last);
            })
            .OrderByDescending(t => t.LastMessage.SentAt)
            .ToList();
    }

    public (bool Success, string ErrorMessage) DeleteMessage(int messageId, int userId)
    {
        var msg = _context.DirectMessages.FirstOrDefault(m => m.Id == messageId);
        if (msg == null)
            return (false, "Message not found");

        if (msg.SenderId != userId)
            return (false, "Not allowed to delete this message");

        _context.DirectMessages.Remove(msg);
        _context.SaveChanges();

        return (true, "");
    }
}
