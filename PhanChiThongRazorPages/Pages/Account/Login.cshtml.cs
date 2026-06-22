using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace PhanChiThongRazorPages.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ISystemAccountService _accountService;
        private readonly IConfiguration _configuration;

        public LoginModel(ISystemAccountService accountService, IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // If already logged in, redirect based on role
            var role = HttpContext.Session.GetInt32("Role");
            if (role == 0) return RedirectToPage("/Admin/Dashboard");
            if (role == 1 || role == 2) return RedirectToPage("/Home");
            return Page(); // Not logged in, show login page
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both Email and Password.";
                return Page();
            }

            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];

            if (Email == adminEmail && Password == adminPassword)
            {
                HttpContext.Session.SetInt32("Role", 0);
                HttpContext.Session.SetString("Email", adminEmail);
                return RedirectToPage("/Admin/Dashboard");
            }

            var account = _accountService.GetAll().FirstOrDefault(a => a.AccountEmail == Email && a.AccountPassword == Password);
            if (account != null)
            {
                HttpContext.Session.SetInt32("Role", account.AccountRole ?? -1);
                HttpContext.Session.SetString("Email", account.AccountEmail);
                HttpContext.Session.SetString("AccountId", account.AccountId.ToString());

                if (account.AccountRole == 1 || account.AccountRole == 2)
                    return RedirectToPage("/Home");
                
                ErrorMessage = "Role not authorized.";
                return Page();
            }

            ErrorMessage = "Invalid email or password.";
            return Page();
        }
    }
}
