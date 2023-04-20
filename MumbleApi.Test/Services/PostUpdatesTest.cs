using System.Text;
using System.Text.Json;

using MumbleApi.Models;

namespace MumbleApi.Test.Services;

public class PostUpdatesTest : IClassFixture<WebAppFactory>
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly WebAppFactory _factory;

    public PostUpdatesTest(WebAppFactory factory)
    {
        _factory = factory;
    }

    public static TheoryData<HttpMethod, string, HttpContent?, Func<ServerEvent<Post>, bool>> ReceiveCorrectPostEventData => new()
    {
        // Create a new normal post.
        {
            HttpMethod.Post, "/posts", new MultipartFormDataContent
            {
                {
                    new StringContent("new post text"), "text"
                },
            },
            result => result is { EventType: "postCreated", Data.Text: "new post text" }
        },

        // Update (replace) a post.
        {
            HttpMethod.Put, "/posts/00000000000000000000000001", new MultipartFormDataContent
            {
                {
                    new StringContent("new post text"), "text"
                },
            },
            result => result is { EventType: "postUpdated", Data.Text: "new post text" }
        },

        // Patch an existing post.
        {
            HttpMethod.Patch, "/posts/00000000000000000000000001", new StringContent(@"{""text"": ""new post text""}", Encoding.UTF8, "application/json"),
            result => result is { EventType: "postUpdated", Data.Text: "new post text" }
        },

        // Update media of a post.
        {
            HttpMethod.Put, "/posts/00000000000000000000000001/media", new MultipartFormDataContent
            {
                {
                    new StreamContent(File.OpenRead("./TestFiles/test_small.png"))
                    {
                        Headers =
                        {
                            ContentType = new("image/png"),
                        },
                    },
                    "media", "test_small.png"
                },
            },
            result => result is { EventType: "postUpdated", Data.MediaUrl: not null }
        },

        // Delete media from a post.
        {
            HttpMethod.Delete, "/posts/00000000000000000000000003/media", null, result => result is { EventType: "postUpdated", Data.MediaUrl: null }
        },

        // Create a reply.
        {
            HttpMethod.Post, "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
            {
                {
                    new StringContent("new reply text"), "text"
                },
            },
            result => result is { EventType: "postCreated", Data.Text: "new reply text" }
        },
    };

    public static TheoryData<HttpMethod, string, HttpContent?, Func<ServerEvent<ServerLikeEvent>, bool>> ReceiveCorrectPostLikeEventData => new()
    {
        // Like a post.
        {
            HttpMethod.Put, "/posts/00000000000000000000000004/likes", null, result => result is
            {
                EventType: "postLiked", Data: { PostId: "00000000000000000000000004", UserId: "1337" },
            }
        },

        // Unlike a post.
        {
            HttpMethod.Delete, "/posts/00000000000000000000000001/likes", null, result => result is
            {
                EventType: "postUnliked", Data: { PostId: "00000000000000000000000001", UserId: "1337" },
            }
        },
    };

    [Theory]
    [MemberData(nameof(ReceiveCorrectPostEventData))]
    public async Task ReceiveCorrectPostEvent(
        HttpMethod method,
        string url,
        HttpContent? content,
        Func<ServerEvent<Post>, bool> verify)
    {
        await _factory.PrepareTestData(TestData.PostsWithLikes);
        var client = _factory.CreateClient();
        var eventTask = GetServerEvent<Post>(
            client,
            "/posts/_sse");
        using var request = new HttpRequestMessage(
            method,
            url)
        {
            Content = content,
        };

        await client.SendAsync(request);

        var result = await eventTask;

        Assert.True(verify(result));
    }

    [Theory]
    [MemberData(nameof(ReceiveCorrectPostLikeEventData))]
    public async Task ReceiveCorrectPostLikeEvent(
        HttpMethod method,
        string url,
        HttpContent? content,
        Func<ServerEvent<ServerLikeEvent>, bool> verify)
    {
        await _factory.PrepareTestData(TestData.PostsWithLikes);
        var client = _factory.CreateClient();
        var eventTask = GetServerEvent<ServerLikeEvent>(
            client,
            "/posts/_sse");
        using var request = new HttpRequestMessage(
            method,
            url)
        {
            Content = content,
        };

        await client.SendAsync(request);

        var result = await eventTask;

        Assert.True(verify(result));
    }

    [Fact]
    public async Task ReceiveCorrectPostDeletedEvent()
    {
        await _factory.PrepareTestData(TestData.PostsWithLikes);
        var client = _factory.CreateClient();
        var eventTask = GetServerEvent<ServerDeleteEvent>(
            client,
            "/posts/_sse");
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            "/posts/00000000000000000000000001");

        await client.SendAsync(request);

        var result = await eventTask;

        Assert.True(result is { EventType: "postDeleted", Data.Id: "00000000000000000000000001" });
    }

    private static async Task<ServerEvent<T>> GetServerEvent<T>(HttpClient client, string url)
    where T : new()
    {
        using var sr = new StreamReader(await client.GetStreamAsync(url));
        if (sr.EndOfStream)
        {
            throw new EndOfStreamException();
        }

        var ev = new ServerEvent<T>();
        while (!sr.EndOfStream)
        {
            switch (await sr.ReadLineAsync())
            {
                case "":
                    return ev;
                case null:
                    throw new EndOfStreamException();
                case var line when line.StartsWith("event:"):
                    ev.EventType = line.Replace("event:", string.Empty).Trim();
                    break;
                case var line when line.StartsWith("data:"):
                    var data = line.Replace("data:", string.Empty).Trim();
                    ev.Data = JsonSerializer.Deserialize<T>(data, Json) ?? throw new JsonException("could not parse json.");
                    break;
            }
        }

        throw new EndOfStreamException();
    }

    public record ServerLikeEvent
    {
        public string UserId { get; set; } = string.Empty;

        public string PostId { get; set; } = string.Empty;
    }

    public record ServerDeleteEvent
    {
        public string Id { get; set; } = string.Empty;
    }

    public record ServerEvent<T>
    where T : new()
    {
        public string? EventType { get; set; }

        public T Data { get; set; } = new();
    }
}
