namespace MechanicsSoftware.Domain.Shared;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
