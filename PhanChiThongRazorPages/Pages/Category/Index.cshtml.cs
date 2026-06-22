using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System.Collections.Generic;
using System.Linq;

namespace PhanChiThongRazorPages.Pages.Category
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly INewsArticleService _newsArticleService;

        public IndexModel(ICategoryService categoryService, INewsArticleService newsArticleService)
        {
            _categoryService = categoryService;
            _newsArticleService = newsArticleService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public IList<PhanChiThong_DataAccess.Models.Category> Categories { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return RedirectToPage("/Account/Login");
            
            var categories = _categoryService.GetAll();
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                categories = categories.Where(c => c.CategoryName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }
            Categories = categories.ToList();
            
            return Page();
        }

        public IActionResult OnPostSaveCategory(PhanChiThong_DataAccess.Models.Category category)
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return Unauthorized();

            if (string.IsNullOrWhiteSpace(category.CategoryName) || string.IsNullOrWhiteSpace(category.CategoryDesciption))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields (Name, Description).";
                return RedirectToPage("/Category/Index");
            }

            var existing = _categoryService.GetById(category.CategoryId);
            if (existing == null)
            {
                _categoryService.Add(category);
            }
            else
            {
                existing.CategoryName = category.CategoryName;
                existing.CategoryDesciption = category.CategoryDesciption;
                existing.IsActive = category.IsActive;
                _categoryService.Update(existing);
            }

            return RedirectToPage("/Category/Index");
        }

        public IActionResult OnPostDeleteCategory(short id)
        {
            if (HttpContext.Session.GetInt32("Role") != 1) return Unauthorized();

            // Check if category is used in NewsArticle
            var articles = _newsArticleService.GetAll().Where(a => a.CategoryId == id).ToList();
            if (articles.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category because it is already used in a news article.";
                return RedirectToPage("/Category/Index");
            }

            var cat = _categoryService.GetById(id);
            if (cat != null) _categoryService.Delete(cat);

            return RedirectToPage("/Category/Index");
        }
    }
}
