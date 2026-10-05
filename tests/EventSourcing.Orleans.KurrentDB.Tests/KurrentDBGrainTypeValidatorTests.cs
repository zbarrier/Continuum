using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class KurrentDBGrainTypeValidatorTests
{
    private const string Kurrent = "Kurrent";

    public sealed class State;

    public sealed class Event;

    public sealed class DefaultProviderGrain : JournaledGrain<State, Event>;

    [LogConsistencyProvider(ProviderName = Kurrent)]
    public sealed class NamedProviderGrain : JournaledGrain<State, Event>;

    [LogConsistencyProvider(ProviderName = Kurrent)]
    public abstract class NamedProviderBaseGrain : JournaledGrain<State, Event>;

    public sealed class InheritedProviderGrain : NamedProviderBaseGrain;

    [LogConsistencyProvider(ProviderName = "Other")]
    public sealed class OtherProviderGrain : JournaledGrain<State, Event>;

    public sealed class PlainGrain : Grain;

    private static List<string> Find(string providerName, params (string GrainType, Type GrainClass)[] grains) =>
        KurrentDBGrainTypeValidator.FindOffendingGrainTypes(grains, providerName);

    [Fact]
    public void Separator_In_A_Grain_Stored_By_The_Default_Provider_Is_Reported()
    {
        var offending = Find(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, ("snack-grain", typeof(DefaultProviderGrain)));

        Assert.Equal($"snack-grain ({typeof(DefaultProviderGrain).FullName})", Assert.Single(offending));
    }

    [Fact]
    public void Separator_In_A_Grain_Stored_By_A_Named_Provider_Is_Reported()
    {
        var offending = Find(Kurrent, ("snack-grain", typeof(NamedProviderGrain)), ("other-grain", typeof(InheritedProviderGrain)));

        Assert.Equal(2, offending.Count);
        Assert.StartsWith("other-grain", offending[0]);
    }

    [Fact]
    public void Grains_Without_The_Separator_Are_Not_Reported()
    {
        Assert.Empty(Find(Kurrent, ("snack.grain", typeof(NamedProviderGrain))));
    }

    [Fact]
    public void Grains_Stored_By_Another_Provider_Are_Not_Reported()
    {
        Assert.Empty(Find(Kurrent, ("other-grain", typeof(OtherProviderGrain)), ("default-grain", typeof(DefaultProviderGrain))));
    }

    [Fact]
    public void Non_Journaled_Grains_Are_Not_Reported()
    {
        Assert.Empty(Find(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, ("plain-grain", typeof(PlainGrain))));
    }

    [Fact]
    public void ValidateConfiguration_Passes_When_No_Grain_Manifest_Is_Registered()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        using var provider = services.BuildServiceProvider();

        new KurrentDBGrainTypeValidator(provider, Kurrent).ValidateConfiguration();
    }

    [Fact]
    public void Default_StreamNameFormatter_Is_The_Shared_Instance()
    {
        Assert.Same(KurrentDBLogConsistentStorageOptions.DefaultStreamNameFormatter, new KurrentDBLogConsistentStorageOptions().StreamNameFormatter);
    }
}
