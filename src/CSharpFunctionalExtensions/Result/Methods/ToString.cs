namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure({Error.GetFormattedMessage()})";
    }
}


public partial struct Result<T>
{
    public override string ToString()
    {
        return IsSuccess ? $"Success({Value})" : $"Failure({Error.GetFormattedMessage()})";
    }
}
