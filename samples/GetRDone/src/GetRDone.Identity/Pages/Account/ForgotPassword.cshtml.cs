using System.ComponentModel.DataAnnotations;
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
public sealed class ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender) : PageModel
{
    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool Submitted { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Always show the same result so the page doesn't reveal which emails have accounts.
        var user = await userManager.FindByEmailAsync(Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await userManager.GeneratePasswordResetTokenAsync(user)));
            var link = Url.Page("/Account/ResetPassword", pageHandler: null, values: new { code, returnUrl = ReturnUrl }, protocol: Request.Scheme)!;
            await emailSender.SendPasswordResetLinkAsync(user, Email, link);
        }

        Submitted = true;
        return Page();
    }
}
