using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;



namespace Socialnetwork.Entityframework
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // viktigt: SQLite här måste matcha din connection string
            optionsBuilder.UseSqlite("Data Source=../SocialNetwork/app.db");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}