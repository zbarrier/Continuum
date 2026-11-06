using GetRDone.Web.Components;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

using ManagedCode.Orleans.Identity.Client.Extensions;

using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["IDENTITY_URL"]
            ?? throw new InvalidOperationException("IDENTITY_URL was not provided.");
        options.ClientId = "getrdone-web";
        options.ClientSecret = "getrdone-web-secret";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("getrdone");
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Copies the authenticated user into the Orleans RequestContext so the silo can authorize grain calls.
builder.Services.AddOrleansIdentity();

builder.AddKeyedAzureTableServiceClient("clustering");
builder.UseOrleansClient(client => client.AddActivityPropagation());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" }));
app.MapPost("/logout", () => TypedResults.SignOut(new AuthenticationProperties { RedirectUri = "/" },
    [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
