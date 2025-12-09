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
    public void SendMessage_ShouldThrow_WhenMessageIsEmpty()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        context.Users.AddRange(new User { Id = 1, Username = "u1" }, new User { Id = 2, Username = "u2" });
        context.SaveChanges();
        var service = new DirectMessageService(context);

        var result = await service.SendMessage(new DirectMessage { Message = "", SenderId = 1, ReceiverId = 2 });
        var dm = new DirectMessage
        {
            SenderId = 1,
            ReceiverId = 2,
            Message = " "
        };

        // act + assert
        var ex = Assert.Throws<ArgumentException>(() => service.SendMessage(dm));
        Assert.Equal("Message cannot be empty", ex.Message);
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
    public void SendMessage_ShouldThrow_WhenMessageLongerThan500Chars()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        var longMessage = new string('a', 501);

        var dm = new DirectMessage
        {
            SenderId = 1,
            ReceiverId = 2,
            Message = longMessage
        };

        // act + assert
        var ex = Assert.Throws<ArgumentException>(() => service.SendMessage(dm));
        Assert.Equal("Message cannot exceed 500 characters", ex.Message);
    }
    [Fact]
    public void SendMessage_ShouldSucceed_WhenMessageIsValid()
    {
        // arrange
        using var context = AppDbContextInMemoryFactory.Create();
        var service = new DirectMessageService(context);

        var dm = new DirectMessage
        {
            SenderId = 1,
            ReceiverId = 2,
            Message = "Tja, läget?"
        };

        // act
        var result = service.SendMessage(dm);

        // assert
        Assert.True(result.Success);
        Assert.Equal("", result.ErrorMessage);
        Assert.Single(context.DirectMessages);
        Assert.NotEqual(default, dm.SentAt);
    }



}
