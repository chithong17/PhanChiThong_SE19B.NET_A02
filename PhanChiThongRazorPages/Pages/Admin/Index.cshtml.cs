using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System.Collections.Generic;

namespace PhanChiThongRazorPages.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ISystemAccountService _accountService;

        public IndexModel(ISystemAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public IList<SystemAccount> Accounts { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return RedirectToPage("/Account/Login");
            
            var accounts = _accountService.GetAll();
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                accounts = accounts.Where(a => 
                    (a.AccountName != null && a.AccountName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase)) ||
                    (a.AccountEmail != null && a.AccountEmail.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            Accounts = accounts.ToList();
            
            return Page();
        }

        public IActionResult OnPostSaveAccount(SystemAccount account)
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return Unauthorized();
            
            if (string.IsNullOrWhiteSpace(account.AccountName) || string.IsNullOrWhiteSpace(account.AccountEmail) || string.IsNullOrWhiteSpace(account.AccountPassword))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields (Name, Email, Password).";
                return RedirectToPage("/Admin/Index");
            }
            
            var existing = _accountService.GetById(account.AccountId);
            if (existing == null)
            {
                _accountService.Add(account);
            }
            else
            {
                existing.AccountName = account.AccountName;
                existing.AccountEmail = account.AccountEmail;
                existing.AccountRole = account.AccountRole;
                existing.AccountPassword = account.AccountPassword;
                _accountService.Update(existing);
            }
            return RedirectToPage("/Admin/Index");
        }

        public IActionResult OnPostDeleteAccount(short id)
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return Unauthorized();
            
            var acc = _accountService.GetById(id);
            if (acc != null) _accountService.Delete(acc);
            
            return RedirectToPage("/Admin/Index");
        }
    }
}
