using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions;

public abstract class Error
{
    protected Error(int priorityCode)
    {
        PriorityCode = priorityCode;
    }

    [JsonInclude]
    public int PriorityCode { get; }

    [JsonIgnore]
    public abstract bool SupportsFormattedMessage { get; }
    public abstract string GetFormattedMessage();

    public new abstract string ToString();
}
