using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;

namespace PhanChiThongRazorPages.Pages.Staff
{
    public class ProfileModel : PageModel
    {
        private readonly ISystemAccountService _accountService;

        public ProfileModel(ISystemAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public SystemAccount Account { get; set; }

        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("Role");
            if (role != 1) return RedirectToPage("/Account/Login");

            var accountId = short.Parse(HttpContext.Session.GetString("AccountId") ?? "0");
            Account = _accountService.GetById(accountId);
            if (Account != null) Account.AccountPassword = ""; // Don't send password to HTML

            return Page();
        }

        public IActionResult OnPost()
        {
            var role = HttpContext.Session.GetInt32("Role");
            if (role != 1) return Unauthorized();

            var realId = short.Parse(HttpContext.Session.GetString("AccountId") ?? "0");
            var existing = _accountService.GetById(realId);
            if (existing != null)
            {
                existing.AccountName = Account.AccountName;
                existing.AccountEmail = Account.AccountEmail;
                if (!string.IsNullOrEmpty(Account.AccountPassword))
                {
                    existing.AccountPassword = Account.AccountPassword;
                }
                
                _accountService.Update(existing);
                SuccessMessage = "Profile updated successfully!";
                
                // Update session email if changed
                HttpContext.Session.SetString("Email", existing.AccountEmail);
            }

            return Page();
        }
    }
}
