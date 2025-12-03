using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Entity.Models;

namespace Socialnetwork.Entityframework
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Follow> Followers => Set<Follow>();
        public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    }
}