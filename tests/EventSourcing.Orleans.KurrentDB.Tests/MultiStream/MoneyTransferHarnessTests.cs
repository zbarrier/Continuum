using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream;

/// <summary>
///     Given/When/Then tests for money transfers, written with the grain test harness. The source account is the grain
///     under test and coordinates the transaction; destination accounts are participants, set up with
///     <c>GivenParticipant</c> and checked with <c>ThenParticipant</c>. A failed transfer must leave every account
///     unchanged, which <c>ThenNoChanges</c> checks.
/// </summary>
[Collection(ClusterCollection.Name)]
public class MoneyTransferHarnessTests : EventDrivenGrainTestHarness<IBankAccountGrain, BankAccountGrain, BankAccountState, BankAccountEvent>
{
    private static readonly DateTimeOffset At = new(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);

    private readonly Guid _sourceAccountId;
    private readonly Guid _destination1AccountId = Guid.NewGuid();
    private readonly Guid _destination2AccountId = Guid.NewGuid();

    public MoneyTransferHarnessTests(ClusterFixture fixture) : this(fixture, Guid.NewGuid()) { }

    private MoneyTransferHarnessTests(ClusterFixture fixture, Guid sourceAccountId) : base(fixture.Cluster, sourceAccountId.ToString())
    {
        _sourceAccountId = sourceAccountId;
    }

    private IBankAccountGrain Account => Cluster.GrainFactory.GetGrain<IBankAccountGrain>(GrainId);

    private static BankAccountOpenedEvent Opened(Guid accountId, decimal openingBalance = 100m, decimal overdraftLimit = 0m) =>
        new(accountId, openingBalance, overdraftLimit, At);

    private static BankAccountClosedEvent Closed(Guid accountId) => new(accountId, At);

    private static IReadOnlyList<object> Events(IReadOnlyList<IChange> changes) =>
        changes.Select(change => Assert.IsType<Change>(change).Event).ToArray();

    [Fact]
    public async Task Transfer_Debits_The_Source_And_Credits_The_Destination()
    {
        await Given(Opened(_sourceAccountId));
        await GivenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), Opened(_destination1AccountId, openingBalance: 0m));

        await When(() => Account.TransferAsync(new TransferMoneyCommand(_destination1AccountId, 40m)));

        Guid transferId = default;
        Then<TransferMoneyResponse>(changes =>
        {
            var debited = Assert.IsType<MoneyDebitedEvent>(Assert.Single(Events(changes)));
            Assert.Equal(_sourceAccountId, debited.AccountId);
            Assert.Equal(_destination1AccountId, debited.ToAccountId);
            Assert.Equal(40m, debited.Amount);
            transferId = debited.TransferId;
        });
        await ThenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), events =>
        {
            var credited = Assert.IsType<MoneyCreditedEvent>(Assert.Single(events));
            Assert.Equal(_destination1AccountId, credited.AccountId);
            Assert.Equal(_sourceAccountId, credited.FromAccountId);
            Assert.Equal(transferId, credited.TransferId);
            Assert.Equal(40m, credited.Amount);
        });
    }

    [Fact]
    public async Task PayAll_Credits_A_Repeated_Destination_Once_Per_Transfer()
    {
        await Given(Opened(_sourceAccountId));
        await GivenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), Opened(_destination1AccountId, openingBalance: 0m));
        await GivenParticipant<IBankAccountGrain>(_destination2AccountId.ToString(), Opened(_destination2AccountId, openingBalance: 0m));

        await When(() => Account.PayAllAsync(new PayAllCommand(
        [
            new TransferMoneyCommand(_destination1AccountId, 10m),
            new TransferMoneyCommand(_destination2AccountId, 20m),
            new TransferMoneyCommand(_destination1AccountId, 30m),
        ])));

        Then<PayAllResponse>(changes =>
        {
            var debits = Events(changes).Select(evt => Assert.IsType<MoneyDebitedEvent>(evt)).ToArray();
            Assert.Equal([10m, 20m, 30m], debits.Select(debit => debit.Amount));
        });
        await ThenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), events =>
            Assert.Equal([10m, 30m], events.Select(evt => Assert.IsType<MoneyCreditedEvent>(evt).Amount)));
        await ThenParticipant<IBankAccountGrain>(_destination2AccountId.ToString(), events =>
            Assert.Equal(20m, Assert.IsType<MoneyCreditedEvent>(Assert.Single(events)).Amount));
    }

    [Fact]
    public async Task Transfer_To_A_Closed_Account_Changes_Nothing()
    {
        await Given(Opened(_sourceAccountId));
        await GivenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), Opened(_destination1AccountId), Closed(_destination1AccountId));

        await When(() => Account.TransferAsync(new TransferMoneyCommand(_destination1AccountId, 40m)));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Transfer_To_An_Account_That_Was_Never_Opened_Changes_Nothing()
    {
        await Given(Opened(_sourceAccountId));

        await When(() => Account.TransferAsync(new TransferMoneyCommand(_destination1AccountId, 40m)));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
        await ThenNoChanges();
        await ThenNoChanges<IBankAccountGrain>(_destination1AccountId.ToString());
    }

    [Fact]
    public async Task PayAll_Over_The_Overdraft_Limit_Rolls_Back_Earlier_Transfers()
    {
        await Given(Opened(_sourceAccountId, openingBalance: 50m));
        await GivenParticipant<IBankAccountGrain>(_destination1AccountId.ToString(), Opened(_destination1AccountId, openingBalance: 0m));
        await GivenParticipant<IBankAccountGrain>(_destination2AccountId.ToString(), Opened(_destination2AccountId, openingBalance: 0m));

        // The first transfer is prepared on its destination before the second one exceeds the overdraft limit.
        await When(() => Account.PayAllAsync(new PayAllCommand(
        [
            new TransferMoneyCommand(_destination1AccountId, 40m),
            new TransferMoneyCommand(_destination2AccountId, 20m),
        ])));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
        await ThenNoChanges();
    }
}
