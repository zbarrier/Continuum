using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream;

/// <summary>
///     Given/When/Then tests for a single bank account, written with the grain test harness. Each test starts from
///     events loaded straight into a new grain, runs one command, and checks the events or the error it returns.
/// </summary>
[Collection(ClusterCollection.Name)]
public class BankAccountGrainTests : EventDrivenGrainTestHarness<IBankAccountGrain, BankAccountGrain, BankAccountState, BankAccountEvent>
{
    private static readonly DateTimeOffset At = new(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);

    private readonly Guid _accountId;

    public BankAccountGrainTests(ClusterFixture fixture) : this(fixture, Guid.NewGuid()) { }

    private BankAccountGrainTests(ClusterFixture fixture, Guid accountId) : base(fixture.Cluster, accountId.ToString())
    {
        _accountId = accountId;
    }

    private IBankAccountGrain Account => Cluster.GrainFactory.GetGrain<IBankAccountGrain>(GrainId);

    private BankAccountOpenedEvent Opened(decimal openingBalance = 100m, decimal overdraftLimit = 0m) =>
        new(_accountId, openingBalance, overdraftLimit, At);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;

    private static void AssertValidationTarget(ValidationError error, string target, string code)
    {
        var entry = Assert.Single(error.Entries);
        Assert.Equal(target, entry.Target);
        Assert.Equal(code, entry.Code);
    }

    [Fact]
    public async Task OpenAccount_Opens_A_New_Account()
    {
        await Given();

        await When(() => Account.OpenAsync(new OpenAccountCommand(100m, 50m)));

        Then<OpenAccountResponse>(changes =>
        {
            var opened = Assert.IsType<BankAccountOpenedEvent>(SingleEvent(changes));
            Assert.Equal(_accountId, opened.AccountId);
            Assert.Equal(100m, opened.OpeningBalance);
            Assert.Equal(50m, opened.OverdraftLimit);
        });
    }

    [Fact]
    public async Task OpenAccount_Rejects_An_Account_That_Is_Already_Open()
    {
        await Given(Opened());

        await When(() => Account.OpenAsync(new OpenAccountCommand(100m, 0m)));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task OpenAccount_Rejects_Values_Out_Of_Range()
    {
        await Given();

        await When(() => Account.OpenAsync(new OpenAccountCommand(-1m, 1_001m)));

        ThenError<ValidationError>(error =>
        {
            Assert.Equal(2, error.Entries.Length);
            Assert.Contains(error.Entries, e => e.Target == "OpeningBalance" && e.Code == ValidationErrorCodes.InclusiveBetween);
            Assert.Contains(error.Entries, e => e.Target == "OverdraftLimit" && e.Code == ValidationErrorCodes.InclusiveBetween);
        });
    }

    [Fact]
    public async Task CreditAccount_Credits_An_Open_Account()
    {
        var transferId = Guid.NewGuid();
        var fromAccountId = Guid.NewGuid();
        await Given(Opened());

        await When(() => Account.CreditAsync(new CreditAccountCommand(transferId, fromAccountId, 25m)));

        Then<CreditAccountResponse>(changes =>
        {
            var credited = Assert.IsType<MoneyCreditedEvent>(SingleEvent(changes));
            Assert.Equal(_accountId, credited.AccountId);
            Assert.Equal(transferId, credited.TransferId);
            Assert.Equal(fromAccountId, credited.FromAccountId);
            Assert.Equal(25m, credited.Amount);
        });
    }

    [Fact]
    public async Task CreditAccount_Rejects_A_Closed_Account()
    {
        await Given(Opened(), new BankAccountClosedEvent(_accountId, At));

        await When(() => Account.CreditAsync(new CreditAccountCommand(Guid.NewGuid(), Guid.NewGuid(), 25m)));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task CreditAccount_Rejects_An_Amount_Below_The_Minimum()
    {
        await Given(Opened());

        await When(() => Account.CreditAsync(new CreditAccountCommand(Guid.NewGuid(), Guid.NewGuid(), 0m)));

        ThenError<ValidationError>(error => AssertValidationTarget(error, "Amount", ValidationErrorCodes.InclusiveBetween));
    }

    [Fact]
    public async Task CloseAccount_Closes_An_Open_Account()
    {
        await Given(Opened());

        await When(() => Account.CloseAsync(new CloseAccountCommand()));

        Then<CloseAccountResponse>(changes =>
        {
            var closed = Assert.IsType<BankAccountClosedEvent>(SingleEvent(changes));
            Assert.Equal(_accountId, closed.AccountId);
        });
    }

    [Fact]
    public async Task CloseAccount_Rejects_An_Account_That_Was_Never_Opened()
    {
        await Given();

        await When(() => Account.CloseAsync(new CloseAccountCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task PayAll_Reports_The_Indexed_Path_Of_An_Invalid_Transfer()
    {
        await Given(Opened());

        await When(() => Account.PayAllAsync(new PayAllCommand([
            new TransferMoneyCommand(Guid.NewGuid(), 10m),
            new TransferMoneyCommand(Guid.NewGuid(), 0m)
        ])));

        ThenError<ValidationError>(error => AssertValidationTarget(error, "Transfers[1].Amount", ValidationErrorCodes.InclusiveBetween));
    }
}
