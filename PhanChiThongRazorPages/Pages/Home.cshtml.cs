using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace PhanChiThongRazorPages.Pages
{
    public class HomeModel : PageModel
    {
        private readonly INewsArticleService _newsService;

        public HomeModel(INewsArticleService newsService)
        {
            _newsService = newsService;
        }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> ActiveNews { get; set; } = new List<PhanChiThong_DataAccess.Models.NewsArticle>();

        public void OnGet()
        {
            // Do not need authentication to view active news
            ActiveNews = _newsService.GetAll()
                .Where(n => n.NewsStatus == true && !string.IsNullOrEmpty(n.NewsTitle) && n.NewsTitle.Length > 3)
                .OrderByDescending(n => n.ViewCount)
                .ThenByDescending(n => n.CreatedDate)
                .ToList();
        }
    }
}
