using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Domain.Customers;

public class CustomerTests
{
    // ── fixtures ─────────────────────────────────────────────────────────────

    private const string ValidName  = "João Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidCnpj  = "11.222.333/0001-81";
    private const string ValidEmail = "joao@email.com";
    private const string ValidPhone = "(31) 98765-4321";

    private static Customer Build(
        string name       = ValidName,
        string taxId      = ValidCpf,
        PersonType type   = PersonType.INDIVIDUAL,
        string email      = ValidEmail,
        string phone      = ValidPhone) =>
        Customer.Create(name, taxId, type, email, phone);

    // ── creation — happy path ────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_ReturnsCreatedCustomer()
    {
        var customer = Build();

        Assert.Equal("João Silva", customer.Name);
        Assert.Equal("52998224725", customer.Document.Value);
        Assert.Equal("joao@email.com", customer.Email.Value);
        Assert.NotEqual(Guid.Empty, customer.Id);
    }

    [Fact]
    public void Create_WithCnpj_SetsTypeCompany()
    {
        var customer = Build(taxId: ValidCnpj, type: PersonType.COMPANY);

        Assert.Equal(PersonType.COMPANY, customer.Document.PersonType);
    }

    [Fact]
    public void Create_TrimsNameAndPhone()
    {
        var customer = Build(name: "  Maria  ", phone: "  31987654321  ");

        Assert.Equal("Maria", customer.Name);
    }

    // ── creation — sad path ──────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Create_InvalidName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => Build(name: name));
    }

    [Fact]
    public void Create_NameTooLong_ThrowsDomainException()
    {
        var longName = new string('A', 101);
        Assert.Throws<DomainException>(() => Build(name: longName));
    }

    [Theory]
    [InlineData("1234567890121423")] // 16 digits — too long
    public void Create_InvalidPhone_ThrowsDomainException(string phone)
    {
        Assert.Throws<DomainException>(() => Build(phone: phone));
    }

    [Fact]
    public void Create_InvalidTaxId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Build(taxId: "000.000.000-00"));
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Build(email: "not-an-email"));
    }

    // ── update ───────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidData_ChangesNameEmailPhone()
    {
        var customer = Build();

        customer.Update("Maria Souza", "maria@email.com", "31912345678");

        Assert.Equal("Maria Souza", customer.Name);
        Assert.Equal("maria@email.com", customer.Email.Value);
    }

    [Fact]
    public void Update_DoesNotChangeTaxId()
    {
        var customer = Build();
        var originalTaxId = customer.Document.Value;

        customer.Update("Outro Nome", "outro@email.com", "31912345678");

        Assert.Equal(originalTaxId, customer.Document.Value);
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Update_InvalidName_ThrowsDomainException(string name)
    {
        var customer = Build();
        Assert.Throws<DomainException>(() => customer.Update(name, ValidEmail, ValidPhone));
    }

    // ── identity ─────────────────────────────────────────────────────────────

    [Fact]
    public void TwoCustomers_SameData_HaveDifferentIds()
    {
        var a = Build();
        var b = Build();

        Assert.NotEqual(a.Id, b.Id);
    }
}