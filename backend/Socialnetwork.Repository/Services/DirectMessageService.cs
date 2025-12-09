using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Services;

public class DirectMessageService
{
    private readonly AppDbContext _context;

    public DirectMessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> SendMessage(DirectMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Message))
            throw new ArgumentException("Message cannot be empty", nameof(message.Message));

        if (message.Message.Length > 500)
            throw new ArgumentException("Message cannot exceed 500 characters", nameof(message.Message));

        var senderExists = await _context.Users.AnyAsync(u => u.Id == message.SenderId);
        var receiverExists = await _context.Users.AnyAsync(u => u.Id == message.ReceiverId);
        if (!senderExists || !receiverExists)
            throw new ArgumentException("Sender or receiver does not exist");

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
