namespace MumbleApi.Errors;

public class PostIsAReplyException : Exception
{
    public PostIsAReplyException()
    {
    }

    public PostIsAReplyException(string? message)
        : base(message)
    {
    }

    public PostIsAReplyException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
