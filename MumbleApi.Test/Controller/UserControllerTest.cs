using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using MumbleApi.Models;

namespace MumbleApi.Test.Controller;

public class UserControllerTest : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public UserControllerTest(WebAppFactory factory)
    {
        _factory = factory;
    }

    public static TheoryData<HttpMethod, string, HttpContent?, HttpStatusCode> ErroneousResultData =>
            new()
            {
            // Fetch a user that does not exist.
            {
                HttpMethod.Get, "/users/foobar", null, HttpStatusCode.NotFound
            },

            // Patch a profile with empty firstname.
            {
                HttpMethod.Patch, "/users", JsonContent.Create(new
                {
                    Firstname = string.Empty,
                }),
                HttpStatusCode.BadRequest
            },

            // Patch a profile with empty lastname.
            {
                HttpMethod.Patch, "/users", JsonContent.Create(new
                {
                    Lastname = string.Empty,
                }),
                HttpStatusCode.BadRequest
            },

            // Patch a profile with empty username.
            {
                HttpMethod.Patch, "/users", JsonContent.Create(new
                {
                    Username = string.Empty,
                }),
                HttpStatusCode.BadRequest
            },

            // Upload an avatar that has no data.
            {
                HttpMethod.Put, "/users/avatar", new MultipartFormDataContent(), HttpStatusCode.BadRequest
            },

            // Upload an avatar that contains wrong data.
            {
                HttpMethod.Put, "/users/avatar", new MultipartFormDataContent
                {
                    {
                        new StreamContent(File.OpenRead("./TestFiles/test.json"))
                        {
                            Headers =
                            {
                                ContentType = new("text/json"),
                            },
                        },
                        "media", "test.json"
                    },
                },
                HttpStatusCode.BadRequest
            },

            // Upload an avatar that is too big.
            {
                HttpMethod.Put, "/users/avatar", new MultipartFormDataContent
                {
                    {
                        new StreamContent(File.OpenRead("./TestFiles/test_large.png"))
                        {
                            Headers =
                            {
                                ContentType = new("image/png"),
                            },
                        },
                        "media", "test_large.png"
                    },
                },
                HttpStatusCode.BadRequest
            },

            // Try to follow a nonexistent user.
            {
                HttpMethod.Put, "/users/foobar/followers", null, HttpStatusCode.NotFound
            },

            // Try to unfollow a nonexistent user.
            {
                HttpMethod.Delete, "/users/foobar/followers", null, HttpStatusCode.NotFound
            },
            };

    [Theory]
    [InlineData("PATCH", "/users")]
    [InlineData("PUT", "/users/avatar")]
    [InlineData("DELETE", "/users/avatar")]
    [InlineData("PUT", "/users/id/followers")]
    [InlineData("DELETE", "/users/id/followers")]
    public async Task ReturnsUnauthorizedOnProtectedRoute(string method, string uri)
    {
        var client = _factory.CreateUnauthorizedClient();
        var result = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), uri));
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEmptyUsers()
    {
        await _factory.PrepareTestData(TestData.Empty);
        var client = _factory.CreateClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<PublicUser>>("/users");

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        result.Data.Should().HaveCount(0);
        result.Count.Should().Be(0);
        result.Next.Should().BeNull();
        result.Previous.Should().BeNull();
    }

    [Theory]
    [InlineData("/users")]
    [InlineData("/users/1337")]
    [InlineData("/users/1337/followers")]
    [InlineData("/users/1337/followees")]
    public async Task GetCorrectInformationForAuthStatus(string url)
    {
        async Task Unauthed()
        {
            await _factory.PrepareTestData(TestData.UsersWithFollowers);
            var client = _factory.CreateUnauthorizedClient();

            var result = await client.GetStringAsync(url);
            result.Should().NotContain("firstname");
        }

        async Task Authed()
        {
            await _factory.PrepareTestData(TestData.UsersWithFollowers);
            var client = _factory.CreateClient();

            var result = await client.GetStringAsync(url);
            result.Should().Contain("firstname");
        }

        await Unauthed();
        await Authed();
    }

    [Theory]
    [InlineData("/users?offset=0&limit=1", false, true)]
    [InlineData("/users?offset=1&limit=1", false, false)]
    [InlineData("/users?offset=2&limit=1", true, false)]
    [InlineData("/users/1337/followers?offset=0&limit=1", false, true)]
    [InlineData("/users/1337/followers?offset=1&limit=1", true, false)]
    [InlineData("/users/1337/followees?offset=0&limit=1", true, true)]
    public async Task PaginateCorrectly(string url, bool nextIsNull, bool prevIsNull)
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<User>>(url);

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        Assert.Equal(nextIsNull, result.Next is null);
        Assert.Equal(prevIsNull, result.Previous is null);
    }

    [Fact]
    public async Task UploadAvatar()
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "/users/avatar")
        {
            Content = new MultipartFormDataContent
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
        };

        var result = await client.SendAsync(request);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadAsStringAsync()).Should().StartWith("https://mockstorage");
    }

    [Fact]
    public async Task DeleteAvatar()
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateUserClient(TestData.UserTestyTester.Id);
        var result = await client.DeleteAsync("/users/avatar");
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var profile = await client.GetFromJsonAsync<User>($"/users/{TestData.UserTestyTester.Id}");
        profile?.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfile()
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "/users")
        {
            Content = JsonContent.Create(new
            {
                FirstName = "Test",
                LastName = "User",
                Username = "Test Username",
            }),
        };

        var result = await client.SendAsync(request);
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var profile = await client.GetFromJsonAsync<User>("/users/1337");
        profile?.Firstname.Should().Be("Test");
        profile?.Lastname.Should().Be("User");
        profile?.Username.Should().Be("Test Username");
    }

    [Fact]
    public async Task FollowUser()
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        var result = await client.PutAsync($"/users/{TestData.UserJackJohnson.Id}/followers", null);
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result = await client.PutAsync($"/users/{TestData.UserJackJohnson.Id}/followers", null);
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var followers = await client.GetFromJsonAsync<PaginatedResult<PublicUser>>($"/users/{TestData.UserJackJohnson.Id}/followers");
        followers?.Data.Any(f => f.Id == TestData.DefaultAuthUser.Id).Should().BeTrue();
    }

    [Fact]
    public async Task UnfollowUser()
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        var result = await client.DeleteAsync($"/users/{TestData.UserTestyTester.Id}/followers");
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result = await client.DeleteAsync($"/users/{TestData.UserTestyTester.Id}/followers");
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var followers = await client.GetFromJsonAsync<PaginatedResult<PublicUser>>($"/users/{TestData.UserTestyTester.Id}/followers");
        followers?.Data.Any(f => f.Id == TestData.DefaultAuthUser.Id).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ErroneousResultData))]
    public async Task ErroneousResult(HttpMethod method, string url, HttpContent? content, HttpStatusCode result)
    {
        await _factory.PrepareTestData(TestData.UsersWithFollowers);
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(method, url)
        {
            Content = content,
        };

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(result);
    }
}
