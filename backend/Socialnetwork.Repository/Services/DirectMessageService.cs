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
}
