#pragma warning disable CA1716 // "Shared" is an intentional DDD namespace segment, not a VB.NET keyword collision
namespace MechanicsSoftware.Domain.Shared;
#pragma warning restore CA1716

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
