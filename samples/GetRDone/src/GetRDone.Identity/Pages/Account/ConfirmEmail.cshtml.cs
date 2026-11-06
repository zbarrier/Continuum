using System.Text;

using GetRDone.Identity.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GetRDone.Identity.Pages.Account;

[SecurityHeaders]
[AllowAnonymous]
public sealed class ConfirmEmailModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public bool Confirmed { get; private set; }

    public string? ReturnUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code, string? returnUrl)
    {
        ReturnUrl = returnUrl;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return Page();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Page();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Page();
        }

        Confirmed = (await userManager.ConfirmEmailAsync(user, token)).Succeeded;
        return Page();
    }
}
