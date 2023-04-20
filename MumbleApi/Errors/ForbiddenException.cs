using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class ForbiddenException : Exception
{
    public ForbiddenException()
    {
    }

    protected ForbiddenException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
