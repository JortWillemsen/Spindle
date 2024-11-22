namespace Engine.Exceptions;

public class InvalidGeometryException : Exception
{
    public InvalidGeometryException()
    {
    }

    public InvalidGeometryException(string? message) : base(message)
    {
    }

    public InvalidGeometryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}