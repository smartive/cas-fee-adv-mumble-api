namespace MumbleApi.Errors;

public class PostInvalidException : Exception
{
    public PostInvalidException()
    {
    }

    public PostInvalidException(string? message)
        : base(message)
    {
    }

    public PostInvalidException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
