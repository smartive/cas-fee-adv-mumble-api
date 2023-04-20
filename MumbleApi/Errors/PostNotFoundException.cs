using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class PostNotFoundException : Exception
{
    public PostNotFoundException()
    {
    }

    protected PostNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
