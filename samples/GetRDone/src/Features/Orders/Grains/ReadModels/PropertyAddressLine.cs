namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>Formats an order's property address the way the client read models list it.</summary>
internal static class PropertyAddressLine
{
    /// <summary>The address on one line, such as <c>48 Birch Hollow Rd, Asheville, NC 28803</c>.</summary>
    public static string Format(string street, string city, string state, string zip) => $"{street}, {city}, {state} {zip}";
}
