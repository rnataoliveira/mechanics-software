namespace MechanicsSoftware.Application.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("Invalid credentials.") { }
}
