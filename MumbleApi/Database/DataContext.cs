using Microsoft.EntityFrameworkCore;

using MumbleApi.Entities;

namespace MumbleApi.Database;

public class DataContext(DbContextOptions options) : DbContext(options)
{
#nullable disable

    public DbSet<Post> Posts { get; set; }

    public DbSet<Like> Likes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Follow> Follows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Like>()
            .HasKey(l => new { l.PostId, l.UserId });

        modelBuilder
            .Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Like>()
            .HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Posts)
            .WithOne(p => p.Creator)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<User>()
            .ToTable(t =>
                t.HasCheckConstraint(
                    "chk_avatar_type",
                    @"(avatar_url is null and avatar_media_type is null) or (avatar_url is not null and avatar_media_type is not null)"));

        modelBuilder
            .Entity<Post>()
            .ToTable(t =>
                t.HasCheckConstraint(
                    "chk_media_data",
                    @"(media_url is null and media_type is null) or (media_url is not null and media_type is not null)"));

        modelBuilder
            .Entity<Post>()
            .ToTable(t =>
                t.HasCheckConstraint(
                    "chk_post_content",
                    @"(media_url is not null and media_type is not null) or text is not null"));

        modelBuilder
            .Entity<Post>()
            .HasMany(p => p.Replies)
            .WithOne(p => p.Parent)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Follow>()
            .HasKey(f => new { f.FollowerId, f.FolloweeId });

        modelBuilder
            .Entity<Follow>()
            .HasOne(f => f.Followee)
            .WithMany(u => u.Followees)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Followers)
            .OnDelete(DeleteBehavior.Cascade);
    }

#nullable enable

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidConverter>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention();
}
