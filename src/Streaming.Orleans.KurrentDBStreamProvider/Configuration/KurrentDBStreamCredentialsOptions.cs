using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     The credentials used by the KurrentDB stream provider when talking to KurrentDB.
/// </summary>
/// <remarks>
///     Mirrors <c>KurrentDBGrainStorageCredentialsOptions</c> and
///     <c>KurrentDBLogConsistentStorageCredentialsOptions</c> so that every KurrentDB backed Orleans provider
///     configures credentials the same way.
/// </remarks>
public sealed class KurrentDBStreamCredentialsOptions
{
    /// <summary>
    ///     When <see langword="true" /> the credentials configured on the underlying client are used.
    /// </summary>
    public bool UseDefault { get; set; } = true;

    /// <summary>
    ///     The user name to authenticate with when <see cref="UseDefault" /> is <see langword="false" />.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    ///     The password to authenticate with when <see cref="UseDefault" /> is <see langword="false" />.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    ///     The bearer token to authenticate with. Takes precedence over <see cref="Username" />/<see cref="Password" />.
    /// </summary>
    public string AuthToken { get; set; } = default!;

    /// <summary>
    ///     Converts these options into <see cref="UserCredentials" />, or <see langword="null" /> when the
    ///     client's default credentials should be used.
    /// </summary>
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
