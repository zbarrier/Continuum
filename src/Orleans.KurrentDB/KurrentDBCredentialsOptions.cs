using KurrentDB.Client;

namespace Continuum.Orleans.KurrentDB;

/// <summary>
/// Credentials used by Continuum's KurrentDB Orleans providers when talking to KurrentDB.
/// </summary>
public sealed class KurrentDBCredentialsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the credentials configured on the KurrentDB client should be used.
    /// When <see langword="true"/>, <see cref="Username"/>, <see cref="Password"/>, and <see cref="AuthToken"/> are ignored.
    /// </summary>
    public bool UseDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets the username used together with <see cref="Password"/>.
    /// </summary>
    [Redact]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password used together with <see cref="Username"/>.
    /// </summary>
    [Redact]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the bearer token. When set, it takes precedence over <see cref="Username"/> and <see cref="Password"/>.
    /// </summary>
    [Redact]
    public string? AuthToken { get; set; }

    /// <summary>
    /// Gets a value indicating whether the configured credentials are usable.
    /// </summary>
    public bool IsValid =>
        UseDefault
        || !string.IsNullOrWhiteSpace(AuthToken)
        || (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));

    /// <summary>
    /// Converts these options to <see cref="UserCredentials"/>.
    /// </summary>
    /// <returns><see langword="null"/> when <see cref="UseDefault"/> is <see langword="true"/>; otherwise the configured credentials.</returns>
    /// <exception cref="InvalidOperationException">The explicit credentials are incomplete.</exception>
    public UserCredentials? ToUserCredentials()
    {
        if (UseDefault)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(AuthToken))
        {
            return new UserCredentials(AuthToken);
        }

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException(
                "KurrentDB credentials require either an AuthToken or both a Username and Password when UseDefault is false.");
        }

        return new UserCredentials(Username, Password);
    }
}
