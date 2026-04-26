#pragma warning disable CA1716 // "Shared" is an intentional DDD namespace segment, not a VB.NET keyword collision
namespace MechanicsSoftware.Domain.Shared;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
