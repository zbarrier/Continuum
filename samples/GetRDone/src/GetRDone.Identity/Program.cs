using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;

using GetRDone.Identity;

using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var webBaseUrl = builder.Configuration["WEB_BASE_URL"]
    ?? throw new InvalidOperationException("WEB_BASE_URL was not provided.");

builder.Services.AddIdentityServer()
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryClients(Config.Clients(webBaseUrl))
    .AddTestUsers(Config.Users);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseIdentityServer();

// Minimal demo login. Real applications should use a full login UI (e.g. the Duende UI templates).
app.MapGet("/Account/Login", (string? returnUrl) => Results.Content(LoginPage.Render(returnUrl, error: null), "text/html"));
app.MapPost("/Account/Login", async (HttpContext http, TestUserStore users, IIdentityServerInteractionService interaction) =>
{
    var form = await http.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (!users.ValidateCredentials(username, form["password"].ToString()))
    {
        return Results.Content(LoginPage.Render(returnUrl, "Invalid username or password."), "text/html");
    }

    var user = users.FindByUsername(username);
    await http.SignInAsync(new IdentityServerUser(user.SubjectId) { DisplayName = user.Username, AdditionalClaims = user.Claims });

    return interaction.IsValidReturnUrl(returnUrl) ? Results.Redirect(returnUrl) : Results.Redirect("/");
}).DisableAntiforgery();
app.MapGet("/Account/Logout", async (HttpContext http, string? logoutId, IIdentityServerInteractionService interaction) =>
{
    await http.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
    var context = await interaction.GetLogoutContextAsync(logoutId, http.RequestAborted);
    return Results.Redirect(context?.PostLogoutRedirectUri ?? "/");
});
app.MapGet("/", () => "Get-R-Done Identity");

app.Run();
