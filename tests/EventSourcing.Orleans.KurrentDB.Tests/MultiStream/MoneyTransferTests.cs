using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream;

[Collection(ClusterCollection.Name)]
public class MoneyTransferTests(ClusterFixture fixture)
{
    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private async Task<(IBankAccountGrain Account, Guid Id)> OpenAccountAsync(decimal openingBalance, decimal overdraftLimit = 0)
    {
        var id = Guid.NewGuid();
        var account = fixture.Cluster.GrainFactory.GetGrain<IBankAccountGrain>(id.ToString());
        Assert.True((await account.OpenAsync(new OpenAccountCommand(openingBalance, overdraftLimit))).IsSuccess);
        return (account, id);
    }

    // The destination is credited through a one-way commit notification, so give it a moment to refresh.
    private static async Task<decimal> WaitForBalanceAsync(IBankAccountGrain account, decimal expected)
    {
        var balance = (await account.GetStateAsync()).Balance;
        for (var attempt = 0; attempt < 50 && balance != expected; attempt++)
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
            balance = (await account.GetStateAsync()).Balance;
        }
        return balance;
    }

    [Fact]
    public async Task Should_Debit_The_Source_And_Credit_The_Destination_Together()
    {
        var (checking, _) = await OpenAccountAsync(100m);
        var (savings, savingsId) = await OpenAccountAsync(0m);

        var result = await checking.TransferAsync(new TransferMoneyCommand(savingsId, 40m));

        Assert.True(result.IsSuccess);
        Assert.Equal(60m, (await checking.GetStateAsync()).Balance);
        Assert.Equal(40m, await WaitForBalanceAsync(savings, 40m));
        // The destination's own commands keep working after the refresh.
        Assert.True((await savings.CloseAsync(new CloseAccountCommand())).IsSuccess);
    }

    [Fact]
    public async Task Should_Credit_An_Open_Account_Directly()
    {
        var (savings, _) = await OpenAccountAsync(10m);

        var result = await savings.CreditAsync(new CreditAccountCommand(Guid.NewGuid(), Guid.NewGuid(), 25m));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Changes);
        Assert.Equal(35m, (await savings.GetStateAsync()).Balance);
    }

    [Fact]
    public async Task Should_Reject_A_Credit_To_A_Closed_Account()
    {
        var (savings, _) = await OpenAccountAsync(0m);
        Assert.True((await savings.CloseAsync(new CloseAccountCommand())).IsSuccess);

        var result = await savings.CreditAsync(new CreditAccountCommand(Guid.NewGuid(), Guid.NewGuid(), 25m));

        Assert.True(result.IsFailure);
        Assert.Equal(0m, (await savings.GetStateAsync()).Balance);
    }

    [Fact]
    public async Task Should_Allow_A_Transfer_Within_The_Overdraft_Limit()
    {
        var (checking, _) = await OpenAccountAsync(50m, overdraftLimit: 100m);
        var (savings, savingsId) = await OpenAccountAsync(0m);

        var result = await checking.TransferAsync(new TransferMoneyCommand(savingsId, 120m));

        Assert.True(result.IsSuccess);
        Assert.Equal(-70m, (await checking.GetStateAsync()).Balance);
        Assert.Equal(120m, await WaitForBalanceAsync(savings, 120m));
    }

    [Fact]
    public async Task Should_Reject_A_Transfer_That_Exceeds_The_Overdraft_Limit()
    {
        var (checking, _) = await OpenAccountAsync(50m, overdraftLimit: 100m);
        var (savings, savingsId) = await OpenAccountAsync(0m);

        var result = await checking.TransferAsync(new TransferMoneyCommand(savingsId, 151m));

        Assert.True(result.IsFailure);
        Assert.Equal(50m, (await checking.GetStateAsync()).Balance);
        Assert.Equal(0m, (await savings.GetStateAsync()).Balance);
    }

    [Fact]
    public async Task Should_Not_Debit_The_Source_When_The_Destination_Is_Closed()
    {
        var (checking, _) = await OpenAccountAsync(100m);
        var (savings, savingsId) = await OpenAccountAsync(0m);
        Assert.True((await savings.CloseAsync(new CloseAccountCommand())).IsSuccess);

        var result = await checking.TransferAsync(new TransferMoneyCommand(savingsId, 40m));

        Assert.True(result.IsFailure);
        Assert.Equal(100m, (await checking.GetStateAsync()).Balance);
        // The destination is not stuck in the failed transfer and still accepts its own commands.
        Assert.True((await savings.OpenAsync(new OpenAccountCommand(0m, 0m))).IsSuccess);
    }

    [Fact]
    public async Task Should_Not_Debit_The_Source_When_The_Destination_Does_Not_Exist()
    {
        var (checking, _) = await OpenAccountAsync(100m);

        var result = await checking.TransferAsync(new TransferMoneyCommand(Guid.NewGuid(), 40m));

        Assert.True(result.IsFailure);
        Assert.Equal(100m, (await checking.GetStateAsync()).Balance);
    }

    [Fact]
    public async Task Should_Pay_Several_Accounts_Including_The_Same_Account_Twice()
    {
        var (checking, _) = await OpenAccountAsync(100m);
        var (rent, rentId) = await OpenAccountAsync(0m);
        var (utilities, utilitiesId) = await OpenAccountAsync(0m);

        var result = await checking.PayAllAsync(new PayAllCommand(
        [
            new TransferMoneyCommand(rentId, 30m),
            new TransferMoneyCommand(utilitiesId, 20m),
            new TransferMoneyCommand(rentId, 10m)
        ]));

        Assert.True(result.IsSuccess);
        Assert.Equal(40m, (await checking.GetStateAsync()).Balance);
        Assert.Equal(40m, await WaitForBalanceAsync(rent, 40m));
        Assert.Equal(20m, await WaitForBalanceAsync(utilities, 20m));
    }

    [Fact]
    public async Task Should_Pay_Nothing_When_A_Later_Payment_Exceeds_The_Overdraft_Limit()
    {
        var (checking, _) = await OpenAccountAsync(100m);
        var (rent, rentId) = await OpenAccountAsync(0m);
        var (utilities, utilitiesId) = await OpenAccountAsync(0m);

        // Each payment fits on its own, but the second one no longer fits after the first.
        var result = await checking.PayAllAsync(new PayAllCommand(
        [
            new TransferMoneyCommand(rentId, 70m),
            new TransferMoneyCommand(utilitiesId, 70m)
        ]));

        Assert.True(result.IsFailure);
        Assert.Equal(100m, (await checking.GetStateAsync()).Balance);
        Assert.Equal(0m, (await rent.GetStateAsync()).Balance);
        Assert.Equal(0m, (await utilities.GetStateAsync()).Balance);
        // The account that already prepared a credit is released and accepts new transfers.
        Assert.True((await checking.TransferAsync(new TransferMoneyCommand(rentId, 70m))).IsSuccess);
        Assert.Equal(70m, await WaitForBalanceAsync(rent, 70m));
    }

    [Fact]
    public async Task Should_Append_Debits_And_Credits_In_The_Order_They_Happened()
    {
        var (checking, checkingId) = await OpenAccountAsync(100m);
        var (_, rentId) = await OpenAccountAsync(0m);
        var (_, utilitiesId) = await OpenAccountAsync(0m);

        var result = await checking.PayAllAsync(new PayAllCommand(
        [
            new TransferMoneyCommand(rentId, 30m),
            new TransferMoneyCommand(utilitiesId, 20m),
            new TransferMoneyCommand(rentId, 10m)
        ]));
        Assert.True(result.IsSuccess);

        var accountIds = new[] { checkingId, rentId, utilitiesId };
        var client = SiloServices.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
        var moved = await client.ReadAllAsync(Direction.Backwards, Position.End, maxCount: 500,
                cancellationToken: TestContext.Current.CancellationToken)
            .Where(e => e.Event.EventType.Contains("Money", StringComparison.Ordinal))
            .Select(e => (e.Event.EventType, AccountId: accountIds.FirstOrDefault(id => e.Event.EventStreamId.Contains(id.ToString(), StringComparison.Ordinal)
                || e.Event.EventStreamId.Contains(id.ToString("N"), StringComparison.Ordinal))))
            .Where(e => e.AccountId != Guid.Empty)
            .Take(6)
            .ToListAsync(TestContext.Current.CancellationToken);
        moved.Reverse();

        Assert.Equal(
        [
            ("Debited", checkingId), ("Credited", rentId),
            ("Debited", checkingId), ("Credited", utilitiesId),
            ("Debited", checkingId), ("Credited", rentId)
        ], moved.Select(e => (e.EventType.Contains("Debited", StringComparison.Ordinal) ? "Debited" : "Credited", e.AccountId)));
    }
}
