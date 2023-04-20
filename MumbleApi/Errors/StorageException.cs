using System.Runtime.Serialization;

namespace MumbleApi.Errors;

[Serializable]
public class StorageException : Exception
{
    public StorageException()
    {
    }

    public StorageException(string message)
        : base(message)
    {
    }

    protected StorageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
