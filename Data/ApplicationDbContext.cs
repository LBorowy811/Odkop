using Microsoft.EntityFrameworkCore;
using Odkop.Models;

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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<ForumModerator> ForumModerators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacja Forum -> Category
            modelBuilder.Entity<Forum>()
                .HasOne(f => f.Category)
                .WithMany(c => c.Forums)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja Topic -> Forum
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Forum)
                .WithMany(f => f.Topics)
                .HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja Post -> Topic
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Topic)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja ForumModerator (wiele-do-wielu)
            modelBuilder.Entity<ForumModerator>()
                .HasOne(fm => fm.User)
                .WithMany(u => u.ModeratedForums)
                .HasForeignKey(fm => fm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumModerator>()
                .HasOne(fm => fm.Forum)
                .WithMany(f => f.Moderators)
                .HasForeignKey(fm => fm.ForumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            var seedDate = new DateTime(2024, 1, 1, 12, 0, 0);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123",
                    Email = "admin@example.com",
                    Role = UserRole.Admin,
                    RegisteredAt = seedDate
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Password = "user123",
                    Email = "user@example.com",
                    Role = UserRole.User,
                    RegisteredAt = seedDate
                }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Ogólne", Description = "Dyskusje ogólne", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Programowanie", Description = "Tematy związane z programowaniem", DisplayOrder = 2 }
            );

            modelBuilder.Entity<Forum>().HasData(
                new Forum { Id = 1, Name = "Dyskusje", Description = "Miejsce na luźne rozmowy", CategoryId = 1, DisplayOrder = 1 },
                new Forum { Id = 2, Name = "Nowości", Description = "Aktualności i ogłoszenia", CategoryId = 1, DisplayOrder = 2 },
                new Forum { Id = 3, Name = "C#", Description = "Dyskusje o języku C#", CategoryId = 2, DisplayOrder = 1 },
                new Forum { Id = 4, Name = "JavaScript", Description = "Dyskusje o JavaScript", CategoryId = 2, DisplayOrder = 2 }
            );

            modelBuilder.Entity<Topic>().HasData(
                new Topic
                {
                    Id = 1,
                    Title = "Witamy na forum!",
                    AuthorId = 1,
                    AuthorName = "admin",
                    ForumId = 1,
                    Created = seedDate,
                    IsPinned = true
                },
                new Topic
                {
                    Id = 2,
                    Title = "Pierwszy wątek w C#",
                    AuthorId = 2,
                    AuthorName = "user",
                    ForumId = 3,
                    Created = seedDate
                }
            );

            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Content = "Witamy wszystkich użytkowników na naszym forum!",
                    AuthorId = 1,
                    AuthorName = "admin",
                    TopicId = 1,
                    Created = seedDate
                },
                new Post
                {
                    Id = 2,
                    Content = "Cześć, mam pytanie odnośnie C#...",
                    AuthorId = 2,
                    AuthorName = "user",
                    TopicId = 2,
                    Created = seedDate
                }
            );
        }
    }
}
