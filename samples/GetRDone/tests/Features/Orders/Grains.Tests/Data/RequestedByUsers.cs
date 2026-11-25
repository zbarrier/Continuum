using System.Security.Claims;

using Duende.IdentityModel;

using GetRDone.Orders.Abstractions;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>The signed-in users that make requests in the tests, with the claims GetRDone.Identity issues.</summary>
public static class RequestedByUsers
{
    /// <summary>The claim GetRDone.Identity uses for the user's tenant.</summary>
    public const string TenantIdClaim = "tenant_id";

    // Role names match GetRDone.Identity's GetRDoneRoles.
    private const string ClientRole = "client";

    /// <summary>John Doe, a client user of Lakeview Credit Union.</summary>
    public static ClaimsPrincipal ClientUser1 { get; } = CreateUser(
        "356fb80c-690a-48de-a9fd-0403a71ebd44", "johnDoe", "John", "Doe", "john.doe@lakeviewcu.example", ClientRole);

    /// <summary>Jane Doe, a client user of Harbor National Bank.</summary>
    public static ClaimsPrincipal ClientUser2 { get; } = CreateUser(
        "a89ce62a-ac8c-4023-bd49-19f3ef25bdb8", "janeDoe", "Jane", "Doe", "jane.doe@harbornational.example", ClientRole);

    /// <summary>Tommy Jones, the AMC's Client Services Director, who is not a client user and assigns orders.</summary>
    public static ClaimsPrincipal Director1 { get; } = CreateUser(
        "49a9d592-00b0-43a9-acb2-5da7f8fcee2e", "tommyJones", "Tommy", "Jones", "tommy.jones@summitvaluation.example",
        StaffRoles.ClientServicesDirector);

    /// <summary>Maria Lopez, an AMC coordinator, who works on orders but can't assign them.</summary>
    public static ClaimsPrincipal Coordinator1 { get; } = CreateUser(
        "b6d1e8f3-2a4c-4e7b-9d05-7c3a1f9e2b48", "mariaLopez", "Maria", "Lopez", "maria.lopez@summitvaluation.example",
        StaffRoles.Coordinator);

    /// <summary>Sam Patel, the AMC's tenant admin, who can do anything the Client Services Director can.</summary>
    public static ClaimsPrincipal TenantAdmin1 { get; } = CreateUser(
        "e2f4a6c8-1b3d-4f5a-8c7e-9d0b2a4c6e81", "samPatel", "Sam", "Patel", "sam.patel@summitvaluation.example",
        StaffRoles.TenantAdmin);

    /// <summary>
    ///     A client user with a new ID, for tests of per-user read models that must start empty even though the test cluster
    ///     is shared.
    /// </summary>
    public static ClaimsPrincipal NewClientUser() => CreateUser(
        Guid.NewGuid().ToString(), "newClient", "New", "Client", "new.client@lakeviewcu.example", ClientRole);

    private static ClaimsPrincipal CreateUser(
        string subject, string userName, string givenName, string familyName, string email, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtClaimTypes.Subject, subject),
            new Claim(JwtClaimTypes.PreferredUserName, userName),
            new Claim(JwtClaimTypes.Name, $"{givenName} {familyName}"),
            new Claim(JwtClaimTypes.GivenName, givenName),
            new Claim(JwtClaimTypes.FamilyName, familyName),
            new Claim(JwtClaimTypes.Email, email),
            new Claim(JwtClaimTypes.Role, role),
            new Claim(TenantIdClaim, TestData.TenantId)
        ], "TestAuthentication", JwtClaimTypes.Name, JwtClaimTypes.Role));
    }
}
