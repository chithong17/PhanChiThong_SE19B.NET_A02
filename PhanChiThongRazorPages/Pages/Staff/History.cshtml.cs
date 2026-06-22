using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace PhanChiThongRazorPages.Pages.Staff
{
    public class HistoryModel : PageModel
    {
        private readonly INewsArticleService _newsService;

        public HistoryModel(INewsArticleService newsService)
        {
            _newsService = newsService;
        }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> MyArticles { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("Role");
            if (role != 1) return RedirectToPage("/Account/Login");

            var accountId = short.Parse(HttpContext.Session.GetString("AccountId") ?? "0");
            
            MyArticles = _newsService.GetAll()
                .Where(a => a.CreatedById == accountId)
                .OrderByDescending(a => a.CreatedDate)
                .ToList();

            return Page();
        }
    }
}
