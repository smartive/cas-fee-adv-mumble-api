using MumbleApi.Database;
using MumbleApi.Entities;

namespace MumbleApi.Test;

public static class TestData
{
    public static User DefaultAuthUser => UserMaxMuster;

    public static User UserMaxMuster => new()
    {
        Id = "1337",
        Firstname = "Max",
        Lastname = "Muster",
        Username = "max_muster",
    };

    public static User UserTestyTester => new()
    {
        Id = "42",
        Firstname = "Testy",
        Lastname = "Tester",
        Username = "testy_tester",
        AvatarMediaType = "image/png",
        AvatarUrl = "https://mymockstorage.com/1234-1234-1234",
    };

    public static User UserJackJohnson => new()
    {
        Id = "80085",
        Firstname = "Jack",
        Lastname = "Johnson",
        Username = "the-mighty-jack",
    };

    public static Task Empty(DataContext context) => Task.CompletedTask;

    public static async Task PostsWithoutLikes(DataContext context)
    {
        await Users(context);
        await context.Posts.AddRangeAsync(
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000001"),
                Text = "Hello World! #newstuff",
                CreatorId = "1337",
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000002"),
                MediaType = "image/png",
                MediaUrl = "https://mymockstorage.com/00000000000000000000000002",
                CreatorId = "1337",
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000003"),
                Text = "This is a post with a media file. #images #rock",
                MediaType = "image/png",
                MediaUrl = "https://mymockstorage.com/00000000000000000000000003",
                CreatorId = "1337",
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000004"),
                Text = "Hello Again #newstuff by me!",
                CreatorId = "42",
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000005"),
                Text = "This is a Reply.",
                CreatorId = "80085",
                ParentId = Ulid.Parse("00000000000000000000000002"),
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000006"),
                Text = "This is a reply with a media file. #images #reply",
                MediaType = "image/png",
                MediaUrl = "https://mymockstorage.com/00000000000000000000000006",
                CreatorId = "42",
                ParentId = Ulid.Parse("00000000000000000000000002"),
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000007"),
                Text = "This is a deleted Reply.",
                CreatorId = "80085",
                Deleted = DateTime.UtcNow,
                ParentId = Ulid.Parse("00000000000000000000000002"),
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000008"),
                Text = "This is another #reply.",
                CreatorId = "80085",
                ParentId = Ulid.Parse("00000000000000000000000002"),
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000009"),
                Text = "This is a deleted Post.",
                CreatorId = "42",
                Deleted = DateTime.UtcNow,
            },
            new Post
            {
                Id = Ulid.Parse("00000000000000000000000010"),
                Text = "Hello World! #newstuff Deleted by 1337.",
                CreatorId = "1337",
                Deleted = DateTime.UtcNow,
            });
    }

    public static async Task PostsWithLikes(DataContext context)
    {
        await PostsWithoutLikes(context);
        await context.Likes.AddRangeAsync(
            new Like
            {
                UserId = DefaultAuthUser.Id,
                PostId = Ulid.Parse("00000000000000000000000001"),
            },
            new Like
            {
                UserId = DefaultAuthUser.Id,
                PostId = Ulid.Parse("00000000000000000000000005"),
            },
            new Like
            {
                UserId = UserTestyTester.Id,
                PostId = Ulid.Parse("00000000000000000000000005"),
            });
    }

    public static Task Users(DataContext context) =>
        context.Users.AddRangeAsync(UserMaxMuster, UserTestyTester, UserJackJohnson);

    public static async Task UsersWithFollowers(DataContext context)
    {
        await Users(context);
        await context.Follows.AddRangeAsync(
            new Follow
            {
                FolloweeId = UserMaxMuster.Id,
                FollowerId = UserTestyTester.Id,
            },
            new Follow
            {
                FolloweeId = UserMaxMuster.Id,
                FollowerId = UserJackJohnson.Id,
            },
            new Follow
            {
                FolloweeId = UserTestyTester.Id,
                FollowerId = UserMaxMuster.Id,
            });
    }
}
