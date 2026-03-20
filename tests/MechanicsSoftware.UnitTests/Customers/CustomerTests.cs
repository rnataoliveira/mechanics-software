using FluentAssertions;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Domain.Customers;

public class CustomerTests
{
    private const string ValidName  = "João Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidCnpj  = "11.222.333/0001-81";
    private const string ValidEmail = "joao@email.com";
    private const string ValidPhone = "(31) 98765-4321";

    private static Customer Build(
        Guid id           = default,
        string name       = ValidName,
        string taxId      = ValidCpf,
        PersonType type   = PersonType.INDIVIDUAL,
        string email      = ValidEmail,
        string phone      = ValidPhone) =>
        Customer.Create(
            id == default ? Guid.NewGuid() : id,
            name, taxId, type, email, phone);

    [Fact]
    public void Create_ValidData_ReturnsCreatedCustomer()
    {
        var customer = Build();

        customer.Name.Should().Be("João Silva");
        customer.Document.Value.Should().Be("52998224725");
        customer.Email.Value.Should().Be("joao@email.com");
        customer.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithCnpj_SetsTypeCompany()
    {
        var customer = Build(taxId: ValidCnpj, type: PersonType.COMPANY);

        customer.Document.PersonType.Should().Be(PersonType.COMPANY);
    }

    [Fact]
    public void Create_TrimsNameAndPhone()
    {
        var customer = Build(name: "  Maria  ", phone: "  31987654321  ");

        customer.Name.Should().Be("Maria");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Create_InvalidName_ThrowsDomainException(string name)
    {
        var act = () => Build(name: name);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NameTooLong_ThrowsDomainException()
    {
        var longName = new string('A', 101);
        var act = () => Build(name: longName);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("1234567890121423")]
    public void Create_InvalidPhone_ThrowsDomainException(string phone)
    {
        var act = () => Build(phone: phone);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_InvalidTaxId_ThrowsDomainException()
    {
        var act = () => Build(taxId: "000.000.000-00");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainException()
    {
        var act = () => Build(email: "not-an-email");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ValidData_ChangesNameEmailPhone()
    {
        var customer = Build();

        customer.Update("Maria Souza", "maria@email.com", "31912345678");

        customer.Name.Should().Be("Maria Souza");
        customer.Email.Value.Should().Be("maria@email.com");
    }

    [Fact]
    public void Update_DoesNotChangeTaxId()
    {
        var customer = Build();
        var originalTaxId = customer.Document.Value;

        customer.Update("Outro Nome", "outro@email.com", "31912345678");

        customer.Document.Value.Should().Be(originalTaxId);
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Update_InvalidName_ThrowsDomainException(string name)
    {
        var customer = Build();
        var act = () => customer.Update(name, ValidEmail, ValidPhone);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TwoCustomers_SameData_HaveDifferentIds()
    {
        var a = Build(id: Guid.NewGuid());
        var b = Build(id: Guid.NewGuid());

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Create_WithExplicitId_PreservesId()
    {
        var id = Guid.NewGuid();
        var customer = Build(id: id);

        customer.Id.Should().Be(id);
    }
}