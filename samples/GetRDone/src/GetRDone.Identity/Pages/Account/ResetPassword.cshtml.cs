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
public sealed class ResetPasswordModel(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool Completed { get; private set; }

    public IActionResult OnGet(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return RedirectToPage("/Account/ForgotPassword", new { returnUrl = ReturnUrl });
        }

        Input.Code = code;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Don't reveal whether the account exists.
        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is not null)
        {
            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "This reset link is invalid.");
                return Page();
            }

            var result = await userManager.ResetPasswordAsync(user, token, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }
        }

        Completed = true;
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
