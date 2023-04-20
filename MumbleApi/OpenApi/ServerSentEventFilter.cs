using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using MumbleApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using Zitadel.Authentication;

namespace MumbleApi.OpenApi;

public class ServerSentEventFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        OpenApiSchema GenerateEvent(string name, string description, string eventName, Type data) => new()
        {
            Title = $"{name} Event",
            Description = $"Server Sent Event that contains {description} as the payload.",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "id", new()
                    {
                        Title = "ID of the event.",
                        Format = "uuid",
                        ReadOnly = true,
                    }
                },
                {
                    "event", new()
                    {
                        Title = "Event type.",
                        Format = "string",
                        ReadOnly = true,
                        Enum = new List<IOpenApiAny>
                        {
                            new OpenApiString(eventName),
                        },
                    }
                },
                {
                    "data", context.SchemaGenerator.GenerateSchema(data, context.SchemaRepository)
                },
            },
            Required = new HashSet<string>
            {
                "id",
                "event",
                "data",
            },
        };

        var postCreated = GenerateEvent("Post Created", "a newly created post", "postCreated", typeof(Post));
        var replyCreated = GenerateEvent("Reply Created", "a newly created reply to a post", "postCreated", typeof(Reply));
        var postUpdated = GenerateEvent("Post/Reply Updated", "an updated post", "postUpdated", typeof(PostBase));
        var postDeleted = GenerateEvent("Reply Created", "the id of a deleted post", "postDeleted", typeof(DeletedPost));
        var postLiked = GenerateEvent("Reply Created", "the userid and postid of the liked post", "postLiked", typeof(LikeInfo));
        var postUnliked = GenerateEvent("Reply Created", "the userid and postid of the unliked post", "postUnliked", typeof(LikeInfo));

        swaggerDoc.Paths.Add("/posts/_sse", new()
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                {
                    OperationType.Get, new()
                    {
                        Summary = "Get a stream of new or updated posts via Server Sent Event push.",
                        Description =
                            @"The server sent events contain newly created posts as well as updated posts. 
Depending on the authentication state, the post contain more or less information.

Currently, the following events are supported:
- postCreated (for creating a new post or a reply)
- postUpdated
- postDeleted
- postLiked
- postUnliked

As an example, using the posts server sent events in javascript is done as follows:
```js
const evtSource = new EventSource(""/posts/_sse"");
evtSource.addEventListener('postCreated', (e) => console.log(e.data));
```

You may read more about server sent events in the
[documentation about Using Server Sent Events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events)",
                        Security = new List<OpenApiSecurityRequirement>
                        {
                            new()
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new()
                                        {
                                            Type = ReferenceType.SecurityScheme, Id = "None",
                                        },
                                    },
                                    Array.Empty<string>()
                                },
                            },
                            new()
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new()
                                        {
                                            Type = ReferenceType.SecurityScheme, Id = ZitadelDefaults.AuthenticationScheme,
                                        },
                                    },
                                    Array.Empty<string>()
                                },
                            },
                        },
                        Tags = new List<OpenApiTag>
                        {
                            new()
                            {
                                Name = "Real Time Data", Description = "Routes for real time data/update access. Uses Server Sent Events (SSE).",
                            },
                        },
                        Responses = new()
                        {
                            {
                                "200", new()
                                {
                                    Description = "Event stream that contains new or updated posts.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "text/event-stream", new()
                                            {
                                                Schema = new()
                                                {
                                                    Type = "array",
                                                    Format = "event-stream",
                                                    Items = new()
                                                    {
                                                        OneOf = new List<OpenApiSchema>
                                                        {
                                                            postCreated,
                                                            replyCreated,
                                                            postUpdated,
                                                            postDeleted,
                                                            postLiked,
                                                            postUnliked,
                                                        },
                                                    },
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - postCreated", new()
                                {
                                    Description = "Definition of the postCreated event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = postCreated,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - replyCreated", new()
                                {
                                    Description = "Definition of the replyCreated event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = replyCreated,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - postUpdated", new()
                                {
                                    Description = "Definition of the postUpdated event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = postUpdated,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - postDeleted", new()
                                {
                                    Description = "Definition of the postDeleted event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = postDeleted,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - postLiked", new()
                                {
                                    Description = "Definition of the postLiked event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = postLiked,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "Event - postUnliked", new()
                                {
                                    Description = "Definition of the postUnliked event.",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        {
                                            "application/json", new()
                                            {
                                                Schema = postUnliked,
                                            }
                                        },
                                    },
                                }
                            },
                            {
                                "500", new()
                                {
                                    Description = "Internal Server Error",
                                }
                            },
                        },
                    }
                },
            },
        });
    }
#pragma warning disable SA1629
#pragma warning disable S1144

    private sealed class DeletedPost
    {
        public Ulid Id { get; set; }
    }

    private sealed class LikeInfo
    {
        public Ulid PostId { get; set; }

        /// <example>179944860378202369</example>
        public string UserId { get; set; } = string.Empty;
    }
#pragma warning restore SA1629
#pragma warning restore S1144
}
