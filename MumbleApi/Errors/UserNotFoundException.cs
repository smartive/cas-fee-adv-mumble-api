using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException()
    {
    }

    protected UserNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
