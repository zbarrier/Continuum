namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Returns <c>Success</c> or <c>Failure(message)</c> using the error's default formatted message.
    /// </summary>
    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure({Error.GetFormattedMessage()})";
    }
}


public partial struct Result<T>
{
    /// <summary>
    ///     Returns <c>Success(value)</c> or <c>Failure(message)</c> using the error's default formatted message.
    /// </summary>
    public override string ToString()
    {
        return IsSuccess ? $"Success({Value})" : $"Failure({Error.GetFormattedMessage()})";
    }
}
