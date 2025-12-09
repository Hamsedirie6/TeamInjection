using System;
using SocialNetwork.Services;
using SocialNetwork.Entity.Models;
using SocialNetwork.Test.Factories;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Test.Services;

public class DirectMessageServiceTests
{
    [Fact]
    public async Task SendMessage_ShouldFail_WhenMessageEmpty()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new DirectMessageService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessage(new DirectMessage { Message = "", SenderId = 1, ReceiverId = 2 }));
    }

    [Fact]
    public async Task SendMessage_ShouldSaveToDatabase()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new DirectMessageService(context);

        await service.SendMessage(new DirectMessage { Message = "Hello", SenderId = 1, ReceiverId = 2 });

        Assert.Equal(1, context.DirectMessages.Count());
    }

    [Fact]
    public async Task SendMessage_ShouldSetTimestamp()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new DirectMessageService(context);

        var msg = new DirectMessage { Message = "Hello", SenderId = 1, ReceiverId = 2 };

        await service.SendMessage(msg);

        Assert.True(msg.SentAt != default);
    }

    [Fact]
    public async Task SendMessage_ShouldThrow_WhenUserMissing()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessage(new DirectMessage { Message = "Hello", SenderId = 1, ReceiverId = 2 }));
    }

    [Fact]
    public async Task SendMessage_ShouldFail_WhenMessageTooLong()
    {
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new DirectMessageService(context);
        var longMessage = new string('a', 501);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendMessage(new DirectMessage { Message = longMessage, SenderId = 1, ReceiverId = 2 }));
    }
}
