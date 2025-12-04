using Microsoft.EntityFrameworkCore;
using Odkop.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Odkop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Topic>().HasData(
                new Topic { Id = 1, Title = "Pierwszy temat", Author = "Admin", Created = DateTime.Now },
                new Topic { Id = 2, Title = "Drugi temat", Author = "Użytkownik123", Created = DateTime.Now }
            );

            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "Pierwszy post",
                    Content = "To jest przykładowa treść posta.",
                    Author = "Admin",
                    TopicId = 1,
                    Created = DateTime.Now
                },

                 new Post
                 {
                     Id = 2,
                     Title = "Drugi post",
                     Content = "Druga przykładowa treść posta.",
                     Author = "Użytkownik123",
                     TopicId = 2,
                     Created = DateTime.Now
                 }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123",   
                    Email = "admin@example.com"
                },

                new User
                {
                    Id = 2,
                    Username = "user",
                    Password = "user123",
                    Email = "user@example.com"
                }
            );
        }
    }
}
