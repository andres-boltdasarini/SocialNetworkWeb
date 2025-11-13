using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetworkWeb.Models;

namespace SocialNetworkWeb.Data
{
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

            builder.Entity<Friendship>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasOne(f => f.User)
                    .WithMany(u => u.Friends)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Friend)
                    .WithMany(u => u.FriendOf)
                    .HasForeignKey(f => f.FriendId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => new { f.UserId, f.FriendId })
                    .IsUnique();
            });

    builder.Entity<ApplicationUser>(entity =>
    {
        entity.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(u => u.Bio)
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);
            });
        }
    }
}