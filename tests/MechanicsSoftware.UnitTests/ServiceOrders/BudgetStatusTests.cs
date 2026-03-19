using FluentAssertions;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.ServiceOrders;

public class BudgetStatusTests
{
    [Fact]
    public void Constructor_ValidStatus_SetsValue()
    {
        var status = new BudgetStatus(BudgetStatus.Status.Approved);
        status.Value.Should().Be(BudgetStatus.Status.Approved);
    }

    [Fact]
    public void CreatePending_ReturnsPendingStatus()
    {
        var status = BudgetStatus.CreatePending();
        status.Value.Should().Be(BudgetStatus.Status.Pending);
    }

    [Fact]
    public void TransitionTo_PendingToApproved_Valid()
    {
        var pending = BudgetStatus.CreatePending();
        var approved = pending.TransitionTo(BudgetStatus.Status.Approved);

        approved.Value.Should().Be(BudgetStatus.Status.Approved);
    }

    [Fact]
    public void TransitionTo_PendingToRejected_Valid()
    {
        var pending = BudgetStatus.CreatePending();
        var rejected = pending.TransitionTo(BudgetStatus.Status.Rejected);

        rejected.Value.Should().Be(BudgetStatus.Status.Rejected);
    }

    [Fact]
    public void TransitionTo_ApprovedToRejected_ThrowsDomainException()
    {
        var approved = new BudgetStatus(BudgetStatus.Status.Approved);
        var act = () => approved.TransitionTo(BudgetStatus.Status.Rejected);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition from 'APPROVED' state: it is terminal*");
    }

    [Fact]
    public void TransitionTo_ApprovedToPending_ThrowsDomainException()
    {
        var approved = new BudgetStatus(BudgetStatus.Status.Approved);
        var act = () => approved.TransitionTo(BudgetStatus.Status.Pending);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition from 'APPROVED' state: it is terminal*");
    }

    [Fact]
    public void TransitionTo_RejectedToApproved_ThrowsDomainException()
    {
        var rejected = new BudgetStatus(BudgetStatus.Status.Rejected);
        var act = () => rejected.TransitionTo(BudgetStatus.Status.Approved);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition from 'REJECTED' state: it is terminal*");
    }

    [Fact]
    public void TransitionTo_RejectedToPending_ThrowsDomainException()
    {
        var rejected = new BudgetStatus(BudgetStatus.Status.Rejected);
        var act = () => rejected.TransitionTo(BudgetStatus.Status.Pending);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition from 'REJECTED' state: it is terminal*");
    }

    [Fact]
    public void TransitionTo_SameStatus_ThrowsDomainException()
    {
        var pending = BudgetStatus.CreatePending();
        var act = () => pending.TransitionTo(BudgetStatus.Status.Pending);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition from 'PENDING' to 'PENDING': already in that state.*");
    }

    [Fact]
    public void ToString_PendingStatus_ReturnsUppercase()
    {
        var pending = BudgetStatus.CreatePending();
        pending.ToString().Should().Be("PENDING");
    }

    [Fact]
    public void ToString_ApprovedStatus_ReturnsUppercase()
    {
        var approved = new BudgetStatus(BudgetStatus.Status.Approved);
        approved.ToString().Should().Be("APPROVED");
    }

    [Fact]
    public void ToString_RejectedStatus_ReturnsUppercase()
    {
        var rejected = new BudgetStatus(BudgetStatus.Status.Rejected);
        rejected.ToString().Should().Be("REJECTED");
    }

    [Fact]
    public void Equality_SameStatus_AreEqual()
    {
        var a = BudgetStatus.CreatePending();
        var b = BudgetStatus.CreatePending();

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentStatuses_AreNotEqual()
    {
        var pending = BudgetStatus.CreatePending();
        var approved = new BudgetStatus(BudgetStatus.Status.Approved);

        (pending != approved).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameStatus_SameHash()
    {
        var a = BudgetStatus.CreatePending();
        var b = BudgetStatus.CreatePending();

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentStatus_DifferentHash()
    {
        var pending = BudgetStatus.CreatePending();
        var approved = new BudgetStatus(BudgetStatus.Status.Approved);

        pending.GetHashCode().Should().NotBe(approved.GetHashCode());
    }
}

