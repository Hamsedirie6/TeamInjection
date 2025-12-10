using SocialNetwork.Entity.Models;
using Socialnetwork.Entityframework;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Services;

public class DirectMessageService
{
    private readonly AppDbContext context;

    public DirectMessageService(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> SendMessage(DirectMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Message))
            throw new ArgumentException("Message cannot be empty", nameof(message.Message));

        if (message.Message.Length > 500)
            throw new ArgumentException("Message cannot exceed 500 characters", nameof(message.Message));

        var senderExists = await context.Users.AnyAsync(u => u.Id == message.SenderId);
        var receiverExists = await context.Users.AnyAsync(u => u.Id == message.ReceiverId);
        if (!senderExists || !receiverExists)
            throw new ArgumentException("Sender or receiver does not exist");

        message.SentAt = DateTime.UtcNow;

        context.DirectMessages.Add(message);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public IEnumerable<DirectMessage> GetConversation(int user1, int user2)
    {
        return context.DirectMessages
            .Where(m =>
                (m.SenderId == user1 && m.ReceiverId == user2) ||
                (m.SenderId == user2 && m.ReceiverId == user1))
            .OrderBy(m => m.SentAt)
            .ToList();
    }

    public IEnumerable<DirectMessage> GetUnread(int userId, DateTime? sinceUtc = null, int limit = 100)
    {
        var since = sinceUtc ?? DateTime.UtcNow.AddMinutes(-30);

        return context.DirectMessages
            .Where(m => m.ReceiverId == userId && m.SentAt >= since)
            .OrderBy(m => m.SentAt)
            .Take(limit)
            .ToList();
    }

    public IEnumerable<(int OtherUserId, DirectMessage LastMessage)> GetThreads(int userId)
    {
        var relevantMessages = context.DirectMessages
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

    public async Task<(bool Success, string ErrorMessage)> DeleteMessage(int messageId, int userId)
    {
        var msg = context.DirectMessages.FirstOrDefault(m => m.Id == messageId);
        if (msg == null)
            return (false, "Message not found");

        if (msg.SenderId != userId)
            return (false, "Not allowed to delete this message");

        context.DirectMessages.Remove(msg);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }
}
