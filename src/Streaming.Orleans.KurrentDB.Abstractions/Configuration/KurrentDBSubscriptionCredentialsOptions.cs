using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

public sealed class KurrentDBSubscriptionCredentialsOptions
{
    public bool UseDefault { get; set; } = true;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string AuthToken { get; set; } = default!;

    public UserCredentials? ToUserCredentials()
    {
        if (UseDefault)
        {
            return null;
        }
        return !string.IsNullOrWhiteSpace(AuthToken)
            ? new UserCredentials(AuthToken)
            : new UserCredentials(Username, Password);
    }
}
