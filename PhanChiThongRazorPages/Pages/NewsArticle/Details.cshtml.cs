using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;

namespace PhanChiThongRazorPages.Pages.NewsArticle
{
    public class DetailsModel : PageModel
    {
        private readonly INewsArticleService _newsService;

        public DetailsModel(INewsArticleService newsService)
        {
            _newsService = newsService;
        }

        public PhanChiThong_DataAccess.Models.NewsArticle Article { get; set; }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> TrendingNews { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var allNews = _newsService.GetAll();
            var allActive = allNews.Where(n => n.NewsStatus == true).OrderByDescending(n => n.CreatedDate).ToList();
            
            Article = allActive.FirstOrDefault(a => a.NewsArticleId == id);
            if (Article == null) 
            {
                TempData["ErrorMessage"] = "This article is no longer available or has been disabled by the author.";
                return RedirectToPage("/Home");
            }

            // Safe ViewCount Update: Fetch without Tags graph to avoid DbUpdateException
            var articleToUpdate = _newsService.GetById(id);
            if (articleToUpdate != null)
            {
                articleToUpdate.ViewCount = (articleToUpdate.ViewCount ?? 0) + 1;
                _newsService.Update(articleToUpdate);
                Article.ViewCount = articleToUpdate.ViewCount; // Sync view model
            }

            TrendingNews = allActive.Where(n => n.NewsArticleId != id).OrderByDescending(n => n.ViewCount).Take(5).ToList();

            return Page();
        }
    }
}
