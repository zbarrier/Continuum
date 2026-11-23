namespace GetRDone.Web.Components.Shared;

/// <summary>Formats values the way the web app shows them.</summary>
public static class DisplayFormat
{
    /// <summary>Formats a 10-digit US phone number, digits only as the domain records it, as (540) 555-0142.
    /// Any other value is shown as it is, and null as an empty string.</summary>
    public static string Phone(string? digits) =>
        digits is { Length: 10 } ? $"({digits[..3]}) {digits[3..6]}-{digits[6..]}" : digits ?? "";
}
