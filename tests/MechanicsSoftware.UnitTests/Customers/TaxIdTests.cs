using FluentAssertions;
using Xunit;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Domain.Customers;

public class TaxIdTests
{
    [Fact]
    public void Should_Create_Valid_CPF()
    {
        var taxId = new TaxId("529.982.247-25", PersonType.INDIVIDUAL);

        Assert.Equal("52998224725", taxId.Value);
    }

    [Fact]
    public void Should_Throw_Invalid_CPF_CheckDigit()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("529.982.247-26", PersonType.INDIVIDUAL));
    }

    [Fact]
    public void Should_Reject_All_Equal_CPF()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("111.111.111-11", PersonType.INDIVIDUAL));
    }

    [Fact]
    public void Should_Create_Valid_CNPJ()
    {
        var taxId = new TaxId("04.252.011/0001-10", PersonType.COMPANY);

        Assert.Equal("04252011000110", taxId.Value);
    }

    [Fact]
    public void Should_Throw_Invalid_CNPJ()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("04.252.011/0001-11", PersonType.COMPANY));
    }

    [Fact]
    public void Should_Throw_When_Wrong_Length()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("123", PersonType.INDIVIDUAL));
    }

    [Fact]
    public void Should_Throw_When_CNPJ_Wrong_Length()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("123456", PersonType.COMPANY));
    }

    [Fact]
    public void Should_Reject_All_Equal_CNPJ()
    {
        Assert.Throws<DomainException>(() =>
            new TaxId("11111111111111", PersonType.COMPANY));
    }

    [Fact]
    public void ToString_ReturnsTaxIdValue()
    {
        var taxId = new TaxId("529.982.247-25", PersonType.INDIVIDUAL);
        taxId.ToString().Should().Be("52998224725");
    }
}