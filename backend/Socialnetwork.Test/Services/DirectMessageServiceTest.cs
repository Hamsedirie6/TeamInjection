using System;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;

namespace SocialNetwork.Test.Services;

public class DirectMessageServiceTests
{
    [Fact]
    public void SendMessage_ShouldFail_WhenMessageEmpty()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        Assert.Throws<ArgumentException>(() => service.SendMessage(new DirectMessage { Message = "" }));
    }

    [Fact]
    public void SendMessage_ShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        service.SendMessage(new DirectMessage { Message = "Hello", SenderId = 1, ReceiverId = 2 });

        Assert.Equal(1, context.DirectMessages.Count());
    }

    [Fact]
    public void SendMessage_ShouldSetTimestamp()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        var msg = new DirectMessage { Message = "Hello", SenderId = 1, ReceiverId = 2 };

        service.SendMessage(msg);

        Assert.True(msg.SentAt != default);
    }

    [Fact]
    public void SendMessage_ShouldFail_WhenMessageTooLong()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);
        var longMessage = new string('a', 501);

        Assert.Throws<ArgumentException>(() => service.SendMessage(new DirectMessage { Message = longMessage, SenderId = 1, ReceiverId = 2 }));
    }
}
