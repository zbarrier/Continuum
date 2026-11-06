using System.ComponentModel.DataAnnotations;
using System.Text;

using GetRDone.Identity.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GetRDone.Identity.Pages.Account;

/// <summary>Vendor self-registration. Vendors join the global vendor pool (no tenant); tenants add them to their fee panel later.</summary>
[SecurityHeaders]
[AllowAnonymous]
public sealed class RegisterModel(UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool Registered { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            DisplayName = Input.DisplayName,
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, GetRDoneRoles.Vendor);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await userManager.GenerateEmailConfirmationTokenAsync(user)));
        var link = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { userId = user.Id, code, returnUrl = ReturnUrl }, protocol: Request.Scheme)!;
        await emailSender.SendConfirmationLinkAsync(user, Input.Email, link);

        Registered = true;
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Company or contact name")]
        [StringLength(256)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
