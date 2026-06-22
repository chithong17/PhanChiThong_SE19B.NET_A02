using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhanChiThong_DataAccess.Models;
using System.Linq;

using PhanChiThong_BusinessLogic.Services;

namespace PhanChiThongRazorPages.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ISystemAccountService _accountService;
        private readonly INewsArticleService _newsArticleService;
        private readonly ICategoryService _categoryService;

        public DashboardModel(ISystemAccountService accountService, INewsArticleService newsArticleService, ICategoryService categoryService)
        {
            _accountService = accountService;
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
        }

        public int TotalArticles { get; set; }
        public int TotalViews { get; set; }
        public int TotalStaffs { get; set; }
        public int TotalCategories { get; set; }

        public string PieLabels { get; set; }
        public string PieData { get; set; }
        public string BarLabels { get; set; }
        public string BarData { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return RedirectToPage("/Account/Login");
            
            var allArticles = _newsArticleService.GetAll();
            TotalArticles = allArticles.Count;
            TotalViews = allArticles.Sum(a => a.ViewCount) ?? 0;
            TotalStaffs = _accountService.GetAll().Count(a => a.AccountRole == 1);
            TotalCategories = _categoryService.GetAll().Count;

            var viewsByCategory = _newsArticleService.GetViewsByCategory();
            
            PieLabels = System.Text.Json.JsonSerializer.Serialize(viewsByCategory.Keys);
            PieData = System.Text.Json.JsonSerializer.Serialize(viewsByCategory.Values);

            var articlesByStaff = _newsArticleService.GetArticlesByStaff();

            BarLabels = System.Text.Json.JsonSerializer.Serialize(articlesByStaff.Keys);
            BarData = System.Text.Json.JsonSerializer.Serialize(articlesByStaff.Values);

            return Page();
        }
    }
}
