using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using PhanChiThongRazorPages.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace PhanChiThongRazorPages.Pages.NewsArticle
{
    public class IndexModel : PageModel
    {
        private readonly INewsArticleService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly IHubContext<NewsHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public IndexModel(INewsArticleService newsService, ICategoryService categoryService, ITagService tagService, IHubContext<NewsHub> hubContext, IWebHostEnvironment env)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
            _hubContext = hubContext;
            _env = env;
        }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> Articles { get; set; }
        public IList<PhanChiThong_DataAccess.Models.Category> Categories { get; set; }
        public IList<Tag> AllTags { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTitle { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return RedirectToPage("/Account/Login");

            var list = _newsService.GetAll();
            if (!string.IsNullOrEmpty(SearchTitle))
                list = list.Where(a => a.NewsTitle != null && a.NewsTitle.Contains(SearchTitle, StringComparison.OrdinalIgnoreCase)).ToList();

            Articles = list.OrderBy(n => int.TryParse(n.NewsArticleId, out int val) ? val : int.MaxValue).ToList();
            Categories = _categoryService.GetAll();
            AllTags = _tagService.GetAll();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveArticle(
            PhanChiThong_DataAccess.Models.NewsArticle article,
            List<int> TagIds,
            IFormFile ImageFile,
            bool IsUpdate = false)
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return Unauthorized();

            if (string.IsNullOrWhiteSpace(article.NewsArticleId) || string.IsNullOrWhiteSpace(article.NewsTitle) || string.IsNullOrWhiteSpace(article.Headline))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields (ID, Title, Headline).";
                return RedirectToPage("/NewsArticle/Index");
            }

            string imageUrl = null;

            // Save image if uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "news");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                imageUrl = "/images/news/" + uniqueFileName;
            }

            var selectedTags = _tagService.GetAll().Where(t => TagIds.Contains(t.TagId)).ToList();

            if (!IsUpdate)
            {
                // Auto-generate ID using 'N' + timestamp to ensure uniqueness within nvarchar(20)
                article.NewsArticleId = "N-" + DateTime.Now.ToString("yyMMddHHmmss");

                // Create new
                article.CreatedById = short.Parse(HttpContext.Session.GetString("AccountId") ?? "1");
                article.CreatedDate = DateTime.Now;
                article.ModifiedDate = DateTime.Now;
                article.ViewCount = 0;
                if (imageUrl != null) article.ImageUrl = imageUrl;
                
                article.Tags = selectedTags;

                _newsService.Add(article);
            }
            else
            {
                // Update existing
                var existingCheck = _newsService.GetById(article.NewsArticleId);
                if (existingCheck == null)
                {
                    TempData["ErrorMessage"] = "Article not found or has been deleted.";
                    return RedirectToPage("/NewsArticle/Index");
                }

                article.ModifiedDate = DateTime.Now;
                article.UpdatedById = short.Parse(HttpContext.Session.GetString("AccountId") ?? "1");
                article.ImageUrl = imageUrl; // Can be null, DAO will ignore it
                article.Tags = selectedTags;
                article.ViewCount = existingCheck.ViewCount;

                _newsService.Update(article);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"A staff member just {(IsUpdate ? "updated" : "published")} the article '{article.NewsTitle}'");
            return RedirectToPage("/NewsArticle/Index");
        }

        public async Task<IActionResult> OnPostDeleteArticle(string id)
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return Unauthorized();

            var article = _newsService.GetById(id);
            if (article != null)
            {
                _newsService.Delete(article);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"A staff member just deleted the article '{article.NewsTitle}'");
            }

            return RedirectToPage("/NewsArticle/Index");
        }
    }
}
