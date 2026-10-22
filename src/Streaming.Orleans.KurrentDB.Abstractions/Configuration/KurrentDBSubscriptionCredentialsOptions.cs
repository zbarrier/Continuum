using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

/// <summary>
///     Credentials a subscription uses when connecting to KurrentDB.
/// </summary>
public sealed class KurrentDBSubscriptionCredentialsOptions
{
    /// <summary>
    ///     Whether to use the client's default credentials instead of the ones configured here.
    /// </summary>
    public bool UseDefault { get; set; } = true;

    /// <summary>
    ///     The user name, used with <see cref="Password" /> when no <see cref="AuthToken" /> is set.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    ///     The password for <see cref="Username" />.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    ///     A bearer token, which takes precedence over <see cref="Username" /> and <see cref="Password" />.
    /// </summary>
    public string AuthToken { get; set; } = default!;

    /// <summary>
    ///     Creates the KurrentDB credentials these options describe.
    /// </summary>
    /// <returns>The credentials, or <see langword="null" /> when <see cref="UseDefault" /> is set.</returns>
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
