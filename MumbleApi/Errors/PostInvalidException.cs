using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class PostInvalidException : Exception
{
    public PostInvalidException()
    {
    }

    protected PostInvalidException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
