namespace Continuum.EventSourcing.Orleans.CosmosDB;

public abstract class Response(double requestCharge)
{
    public double RequestCharge { get; } = requestCharge;
}

internal class EventItemResponse(EventItem? eventItem, double requestCharge) : Response(requestCharge)
{
    public bool HasEventItem => EventItem is not null;
    public EventItem? EventItem { get; } = eventItem;
}