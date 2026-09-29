using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Serialization;

namespace Continuum.Orleans.Tests;

public sealed class NewIdSurrogateTests
{
    private static readonly Serializer Serializer = new ServiceCollection()
        .AddSerializer()
        .BuildServiceProvider()
        .GetRequiredService<Serializer>();

    private static T RoundTrip<T>(T value) => Serializer.Deserialize<T>(Serializer.SerializeToArray(value))!;

    [Fact]
    public void NewId_RoundTrips()
    {
        var id = NewId.Next();

        Assert.Equal(id, RoundTrip(id));
    }

    [Fact]
    public void NewId_Empty_RoundTrips()
    {
        Assert.Equal(NewId.Empty, RoundTrip(NewId.Empty));
    }

    [Fact]
    public void NewId_RoundTrip_PreservesTimestamp()
    {
        var id = NewId.Next();

        Assert.Equal(id.Timestamp, RoundTrip(id).Timestamp);
    }

    [Fact]
    public void NewId_RoundTrip_PreservesOrder()
    {
        var ids = NewId.Next(100);

        var roundTripped = ids.Select(RoundTrip).ToArray();

        Assert.Equal(ids, roundTripped);
        for (var i = 0; i < roundTripped.Length - 1; i++)
            Assert.True(roundTripped[i] < roundTripped[i + 1]);
    }

    [Fact]
    public void NullableNewId_WithValue_RoundTrips()
    {
        NewId? id = NewId.Next();

        Assert.Equal(id, RoundTrip(id));
    }

    [Fact]
    public void NullableNewId_Null_RoundTrips()
    {
        Assert.Null(RoundTrip<NewId?>(null));
    }

    [Fact]
    public void NewIdArray_RoundTrips()
    {
        var ids = NewId.Next(10);

        Assert.Equal(ids, RoundTrip(ids));
    }

    [Fact]
    public void NewId_AsMemberOfGeneratedType_RoundTrips()
    {
        var dto = new OrderDto { Id = NewId.Next(), Name = "order" };

        var result = RoundTrip(dto);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Name, result.Name);
    }

    [Fact]
    public void Converter_UsesSequentialGuid()
    {
        var id = NewId.Next();
        var converter = new NewIdSurrogateConverter();

        var surrogate = converter.ConvertToSurrogate(id);

        Assert.Equal(id.ToSequentialGuid(), surrogate.SequentialGuid);
        Assert.Equal(id, converter.ConvertFromSurrogate(surrogate));
    }
}

[GenerateSerializer]
public sealed class OrderDto
{
    [Id(0)] public NewId Id { get; init; }
    [Id(1)] public string Name { get; init; } = "";
}
