namespace Continuum.EventSourcing.Orleans.CosmosDB;

public enum DeleteMode
{
    Soft,
    Hard,
    SetTimeToLive,
}
