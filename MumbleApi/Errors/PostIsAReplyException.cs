using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class PostIsAReplyException : Exception
{
    public PostIsAReplyException()
    {
    }

    protected PostIsAReplyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
