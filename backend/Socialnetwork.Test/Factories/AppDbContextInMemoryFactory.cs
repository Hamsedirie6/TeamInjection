using System;
using Microsoft.EntityFrameworkCore;
using Socialnetwork.Entityframework;

namespace SocialNetwork.Test.Factories;

public static class AppDbContextInMemoryFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unik DB per test
            .Options;

        return new AppDbContext(options);
    }
}
