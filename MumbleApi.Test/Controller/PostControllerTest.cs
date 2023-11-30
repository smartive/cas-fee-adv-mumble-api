using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using FluentAssertions;

using MumbleApi.Models;

namespace MumbleApi.Test.Controller;

public class PostControllerTest(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    public static TheoryData<string, MultipartFormDataContent, Action<PostBase>> CreatePostData =>
            new()
            {
            // Create a post with text only.
            {
                "/posts", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
                },
                result => result.Text.Should().Be("new post text")
            },

            // Create post with media only.
            {
                "/posts", new MultipartFormDataContent
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
                result => { result.MediaUrl.Should().NotBeNull(); }
            },

            // Create post with text and media.
            {
                "/posts", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
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
                result =>
                {
                    result.Text.Should().Be("new post text");
                    result.MediaUrl.Should().NotBeNull();
                }
            },

            // Create reply with text only.
            {
                "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
                },
                result => { result.Text.Should().Be("new post text"); }
            },

            // Create reply with media only.
            {
                "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
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
                result => { result.MediaUrl.Should().NotBeNull(); }
            },

            // Create reply with text and media.
            {
                "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
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
                result =>
                {
                    result.Text.Should().Be("new post text");
                    result.MediaUrl.Should().NotBeNull();
                }
            },
            };

    public static TheoryData<HttpMethod, string, HttpContent?, HttpStatusCode> ErroneousResultData =>
            new()
            {
            // Create a post without data.
            {
                HttpMethod.Post, "/posts", new MultipartFormDataContent(), HttpStatusCode.BadRequest
            },

            // Create a post with wrong media.
            {
                HttpMethod.Post, "/posts", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
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

            // Create a post with wrong media only.
            {
                HttpMethod.Post, "/posts", new MultipartFormDataContent
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

            // Create a post with too large media.
            {
                HttpMethod.Post, "/posts", new MultipartFormDataContent
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

            // Fetch a post that does not exist.
            {
                HttpMethod.Get, "/posts/00000000000000000000000000", null, HttpStatusCode.NotFound
            },

            // Fetch a deleted post.
            {
                HttpMethod.Get, "/posts/00000000000000000000000010", null, HttpStatusCode.NotFound
            },

            // Update a post without content.
            {
                HttpMethod.Put, "/posts/00000000000000000000000001", new MultipartFormDataContent(), HttpStatusCode.BadRequest
            },

            // Update a deleted post.
            {
                HttpMethod.Put, "/posts/00000000000000000000000010", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
                },
                HttpStatusCode.NotFound
            },

            // Update a post that does not belong to the user.
            {
                HttpMethod.Put, "/posts/00000000000000000000000004", new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
                },
                HttpStatusCode.Forbidden
            },

            // Update a post with wrong media.
            {
                HttpMethod.Put, "/posts/00000000000000000000000001", new MultipartFormDataContent
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

            // Update a post with too large media.
            {
                HttpMethod.Put, "/posts/00000000000000000000000001", new MultipartFormDataContent
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

            // Update a post that does not exist.
            {
                HttpMethod.Put, "/posts/00000000000000000000000000", new MultipartFormDataContent
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
                HttpStatusCode.NotFound
            },

            // Patch a post that does not exist.
            {
                HttpMethod.Patch, "/posts/00000000000000000000000000", new StringContent(@"{""text"": ""new text""}", Encoding.Unicode, "application/json"), HttpStatusCode.NotFound
            },

            // Patch a post that does not belong to the user.
            {
                HttpMethod.Patch, "/posts/00000000000000000000000004", new StringContent(@"{""text"": ""new text""}", Encoding.Unicode, "application/json"), HttpStatusCode.Forbidden
            },

            // Patch a post that results in an invalid post.
            {
                HttpMethod.Patch, "/posts/00000000000000000000000001", new StringContent(@"{""text"": """"}", Encoding.Unicode, "application/json"), HttpStatusCode.BadRequest
            },

            // Patch a deleted post.
            {
                HttpMethod.Patch, "/posts/00000000000000000000000010", new StringContent(@"{""text"": ""new text""}", Encoding.Unicode, "application/json"), HttpStatusCode.NotFound
            },

            // Delete a post that does not exist.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000000", null, HttpStatusCode.NotFound
            },

            // Delete a post that does not belong to the user.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000004", null, HttpStatusCode.Forbidden
            },

            // Replace media of a post that does not exist.
            {
                HttpMethod.Put, "/posts/00000000000000000000000000/media", new MultipartFormDataContent
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
                HttpStatusCode.NotFound
            },

            // Replace media of a deleted post.
            {
                HttpMethod.Put, "/posts/00000000000000000000000010/media", new MultipartFormDataContent
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
                HttpStatusCode.NotFound
            },

            // Replace media of a post that does not belong to the user.
            {
                HttpMethod.Put, "/posts/00000000000000000000000004/media", new MultipartFormDataContent
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
                HttpStatusCode.Forbidden
            },

            // Replace media of a post with too big media.
            {
                HttpMethod.Put, "/posts/00000000000000000000000001/media", new MultipartFormDataContent
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

            // Replace media of a post with wrong media type.
            {
                HttpMethod.Put, "/posts/00000000000000000000000001/media", new MultipartFormDataContent
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

            // Delete media from a post that does not exist.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000000/media", null, HttpStatusCode.NotFound
            },

            // Delete a deleted post.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000010/media", null, HttpStatusCode.NotFound
            },

            // Delete media from a post that results in an invalid post.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000002/media", null, HttpStatusCode.BadRequest
            },

            // Delete media from a post that does not belong to the user.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000006/media", null, HttpStatusCode.Forbidden
            },

            // Fetch replies of a post that does not exist.
            {
                HttpMethod.Get, "/posts/00000000000000000000000000/replies", null, HttpStatusCode.NotFound
            },

            // Fetch replies of a post that is a reply.
            {
                HttpMethod.Get, "/posts/00000000000000000000000005/replies", null, HttpStatusCode.BadRequest
            },

            // Fetch replies of a post that is deleted.
            {
                HttpMethod.Get, "/posts/00000000000000000000000010/replies", null, HttpStatusCode.NotFound
            },

            // Create a reply with invalid media.
            {
                HttpMethod.Post, "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
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

            // Create a reply without any content.
            {
                HttpMethod.Post, "/posts/00000000000000000000000001/replies", null, HttpStatusCode.BadRequest
            },

            // Create a reply on a deleted post.
            {
                HttpMethod.Post, "/posts/00000000000000000000000010/replies", new MultipartFormDataContent
                {
                    {
                        new StringContent("new reply"), "text"
                    },
                },
                HttpStatusCode.NotFound
            },

            // Create a reply on a reply.
            {
                HttpMethod.Post, "/posts/00000000000000000000000005/replies", new MultipartFormDataContent
                {
                    {
                        new StringContent("new reply"), "text"
                    },
                },
                HttpStatusCode.BadRequest
            },

            // Create a reply on a post that does not exist.
            {
                HttpMethod.Post, "/posts/00000000000000000000000000/replies", new MultipartFormDataContent
                {
                    {
                        new StringContent("new reply"), "text"
                    },
                },
                HttpStatusCode.NotFound
            },

            // Create a reply with too big media.
            {
                HttpMethod.Post, "/posts/00000000000000000000000001/replies", new MultipartFormDataContent
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

            // Like a post that does not exist.
            {
                HttpMethod.Put, "/posts/00000000000000000000000000/likes", null, HttpStatusCode.NotFound
            },

            // Like a deleted post.
            {
                HttpMethod.Put, "/posts/00000000000000000000000010/likes", null, HttpStatusCode.NotFound
            },

            // Unlike a post that does not exist.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000000/likes", null, HttpStatusCode.NotFound
            },

            // Unlike a deleted post.
            {
                HttpMethod.Delete, "/posts/00000000000000000000000010/likes", null, HttpStatusCode.NotFound
            },
            };

    public static TheoryData<MultipartFormDataContent, Action<PostBase>> ReplacePostData =>
            new()
            {
            // Create a post with text only.
            {
                new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
                },
                result => { result.Text.Should().Be("new post text"); }
            },

            // Create post with media only.
            {
                new MultipartFormDataContent
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
                result => { result.MediaUrl.Should().NotBeNull(); }
            },

            // Create post with text and media.
            {
                new MultipartFormDataContent
                {
                    {
                        new StringContent("new post text"), "text"
                    },
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
                result =>
                {
                    result.Text.Should().Be("new post text");
                    result.MediaUrl.Should().NotBeNull();
                }
            },
            };

    [Theory]
    [InlineData("POST", "/posts")]
    [InlineData("PUT", "/posts/id")]
    [InlineData("PATCH", "/posts/id")]
    [InlineData("DELETE", "/posts/id")]
    [InlineData("PUT", "/posts/id/media")]
    [InlineData("DELETE", "/posts/id/media")]
    [InlineData("POST", "/posts/id/replies")]
    [InlineData("PUT", "/posts/id/likes")]
    [InlineData("DELETE", "/posts/id/likes")]
    public async Task ReturnsUnauthorizedOnProtectedRoute(string method, string uri)
    {
        var client = factory.CreateUnauthorizedClient();
        var result = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), uri));
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FetchEmptyPosts()
    {
        await factory.PrepareTestData(TestData.Empty);
        var client = factory.CreateClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<Post>>("/posts");

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
    [InlineData("/posts?offset=0&limit=2", false, true)]
    [InlineData("/posts?offset=1&limit=2", false, false)]
    [InlineData("/posts?offset=2&limit=2", true, false)]
    [InlineData("/posts/00000000000000000000000002/replies?offset=0&limit=1", false, true)]
    [InlineData("/posts/00000000000000000000000002/replies?offset=1&limit=1", false, false)]
    [InlineData("/posts/00000000000000000000000002/replies?offset=2&limit=1", true, false)]
    public async Task PaginateCorrectly(string url, bool nextIsNull, bool prevIsNull)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<Post>>(url);

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        Assert.Equal(nextIsNull, result.Next is null);
        Assert.Equal(prevIsNull, result.Previous is null);
    }

    [Theory]
    [InlineData("/posts")]
    [InlineData("/posts/00000000000000000000000002/replies")]
    public async Task NoInfoAboutLikeWithoutAuth(string url)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateUnauthorizedClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<Post>>(url);

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        result.Data.Should().AllSatisfy(p => p.LikedBySelf.Should().BeNull());
    }

    [Theory]
    [InlineData("/posts")]
    [InlineData("/posts/00000000000000000000000002/replies")]
    public async Task ContainInfoAboutLikeWithAuth(string url)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var result = await client.GetFromJsonAsync<PaginatedResult<Post>>(url);

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        result.Data.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Theory]
    [MemberData(nameof(CreatePostData))]
    public async Task CreatePost(string url, MultipartFormDataContent content, Action<PostBase> verify)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = content;

        var response = await client.SendAsync(request);
        var result = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<PostBase>();

        if (result is null)
        {
            Assert.Fail("result is null.");
        }

        result.Creator.Id.Should().Be(TestData.DefaultAuthUser.Id);
        result.LikedBySelf.Should().BeFalse();
        verify(result);
    }

    [Theory]
    [MemberData(nameof(ErroneousResultData))]
    public async Task ErroneousResult(HttpMethod method, string url, HttpContent? content, HttpStatusCode result)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(method, url);
        request.Content = content;

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(result);
    }

    [Fact]
    public async Task FetchPostById()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateUnauthorizedClient();
        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        result.LikedBySelf.Should().BeNull();
        result.Text.Should().Be("Hello World! #newstuff");
    }

    [Fact]
    public async Task FetchPostByIdWithLikeInfo()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");

        if (result is null)
        {
            Assert.Fail("Result is null.");
        }

        result.LikedBySelf.Should().BeFalse();
        result.Text.Should().Be("Hello World! #newstuff");
    }

    [Theory]
    [MemberData(nameof(ReplacePostData))]
    public async Task ReplacePost(MultipartFormDataContent content, Action<PostBase> verify)
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, "/posts/00000000000000000000000001");
        request.Content = content;

        var response = await client.SendAsync(request);
        var result = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<PostBase>();

        if (result is null)
        {
            Assert.Fail("result is null.");
        }

        result.LikedBySelf.Should().BeFalse();
        verify(result);
    }

    [Fact]
    public async Task PatchPostWithText()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Patch, "/posts/00000000000000000000000001");
        request.Content = new StringContent("""{"text": "new post text"}""", new MediaTypeHeaderValue("application/json"));

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");
        result?.Text.Should().Be("new post text");
    }

    [Fact]
    public async Task DeleteAPost()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/posts/00000000000000000000000001");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var result = await client.GetAsync("/posts/00000000000000000000000001");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AttachMediaOnPost()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var response = await client.PutAsync("/posts/00000000000000000000000001/media", new MultipartFormDataContent
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
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newUrl = await response.Content.ReadAsStringAsync();
        newUrl.Should().StartWith("https://mockstorage/");
    }

    [Fact]
    public async Task ReplaceMediaOnPost()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var response = await client.PutAsync("/posts/00000000000000000000000002/media", new MultipartFormDataContent
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
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newUrl = await response.Content.ReadAsStringAsync();
        newUrl.Should().StartWith("https://mockstorage/");
    }

    [Fact]
    public async Task DeleteMediaFromPost()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/posts/00000000000000000000000003/media");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");
        result?.MediaUrl.Should().BeNull();
    }

    [Fact]
    public async Task LikePost()
    {
        await factory.PrepareTestData(TestData.PostsWithoutLikes);
        var client = factory.CreateClient();

        // Perform the put twice to see if it's idempotent.
        var response = await client.PutAsync("/posts/00000000000000000000000001/likes", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response = await client.PutAsync("/posts/00000000000000000000000001/likes", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");
        result?.LikedBySelf.Should().BeTrue();
    }

    [Fact]
    public async Task UnlikePost()
    {
        await factory.PrepareTestData(TestData.PostsWithLikes);
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");
        result?.LikedBySelf.Should().BeTrue();

        // Perform the put twice to see if it's idempotent.
        var response = await client.DeleteAsync("/posts/00000000000000000000000001/likes");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response = await client.DeleteAsync("/posts/00000000000000000000000001/likes");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        result = await client.GetFromJsonAsync<Post>("/posts/00000000000000000000000001");
        result?.LikedBySelf.Should().BeFalse();
    }
}
