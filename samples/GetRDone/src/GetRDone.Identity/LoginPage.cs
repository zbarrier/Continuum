using System.Net;

namespace GetRDone.Identity;

internal static class LoginPage
{
    public static string Render(string? returnUrl, string? error)
    {
        var encodedReturnUrl = WebUtility.HtmlEncode(returnUrl ?? string.Empty);
        var errorHtml = error is null ? string.Empty : $"<p style=\"color:#b00\">{WebUtility.HtmlEncode(error)}</p>";
        return $$"""
            <!DOCTYPE html>
            <html>
            <head><title>Get-R-Done sign in</title></head>
            <body style="font-family:sans-serif;max-width:360px;margin:4rem auto">
              <h1>Get-R-Done</h1>
              {{errorHtml}}
              <form method="post" action="/Account/Login">
                <input type="hidden" name="returnUrl" value="{{encodedReturnUrl}}" />
                <p><label>Username<br /><input name="username" autofocus /></label></p>
                <p><label>Password<br /><input name="password" type="password" /></label></p>
                <button type="submit">Sign in</button>
              </form>
              <p><small>Demo users: client, coordinator, reviewer, vendor. Password: {{WebUtility.HtmlEncode(Config.DemoPassword)}}</small></p>
            </body>
            </html>
            """;
    }
}
