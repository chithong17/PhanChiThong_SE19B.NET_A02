using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhanChiThongRazorPages.Pages.Admin
{
    public class ReportModel : PageModel
    {
        private readonly INewsArticleService _newsArticleService;

        public ReportModel(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> Articles { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return RedirectToPage("/Account/Login");
            
            var articles = _newsArticleService.GetAll();
            if (StartDate.HasValue) articles = articles.Where(a => a.CreatedDate >= StartDate.Value).ToList();
            if (EndDate.HasValue) {
                var nextDay = EndDate.Value.AddDays(1);
                articles = articles.Where(a => a.CreatedDate < nextDay).ToList();
            }
            
            Articles = articles.OrderByDescending(a => a.CreatedDate).ToList();
            return Page();
        }

        public IActionResult OnGetExport()
        {
            if (HttpContext.Session.GetInt32("Role") != 0) return Unauthorized();

            var articlesList = _newsArticleService.GetAll();
            if (StartDate.HasValue) articlesList = articlesList.Where(a => a.CreatedDate >= StartDate.Value).ToList();
            if (EndDate.HasValue) {
                var nextDay = EndDate.Value.AddDays(1);
                articlesList = articlesList.Where(a => a.CreatedDate < nextDay).ToList();
            }
            
            var articles = articlesList.OrderByDescending(a => a.CreatedDate).ToList();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("ID,Title,Created Date,Status,Views");

            foreach (var item in articles)
            {
                var id = item.NewsArticleId;
                var title = item.NewsTitle != null ? $"\"{item.NewsTitle.Replace("\"", "\"\"")}\"" : "";
                var date = item.CreatedDate?.ToString("yyyy-MM-dd HH:mm");
                var status = item.NewsStatus == true ? "Active" : "Inactive";
                var views = item.ViewCount ?? 0;
                
                builder.AppendLine($"{id},{title},{date},{status},{views}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "NewsReport.csv");
        }
    }
}
