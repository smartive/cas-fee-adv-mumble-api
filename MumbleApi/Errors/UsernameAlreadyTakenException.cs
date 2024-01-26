﻿namespace MumbleApi.Errors;

public class UsernameAlreadyTakenException : Exception
{
    public UsernameAlreadyTakenException()
    {
    }

    public UsernameAlreadyTakenException(string? message)
        : base(message)
    {
    }

    public UsernameAlreadyTakenException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
