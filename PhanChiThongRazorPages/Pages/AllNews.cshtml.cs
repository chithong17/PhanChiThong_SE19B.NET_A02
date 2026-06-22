using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhanChiThong_BusinessLogic.Services;
using PhanChiThong_DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhanChiThongRazorPages.Pages
{
    public class AllNewsModel : PageModel
    {
        private readonly INewsArticleService _newsService;

        public AllNewsModel(INewsArticleService newsService)
        {
            _newsService = newsService;
        }

        public IList<PhanChiThong_DataAccess.Models.NewsArticle> PagedNews { get; set; } = new List<PhanChiThong_DataAccess.Models.NewsArticle>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public void OnGet(int pageIndex = 1, string query = null)
        {
            SearchQuery = query;
            int pageSize = 8;
            var allActive = _newsService.GetAll()
                .Where(n => n.NewsStatus == true);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var lowerQuery = SearchQuery.ToLower();
                allActive = allActive.Where(n => 
                    (n.NewsTitle != null && n.NewsTitle.ToLower().Contains(lowerQuery)) || 
                    (n.Headline != null && n.Headline.ToLower().Contains(lowerQuery)));
            }

            var sortedActive = allActive.OrderByDescending(n => n.CreatedDate).ToList();

            int totalItems = sortedActive.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (pageIndex < 1) pageIndex = 1;
            if (pageIndex > TotalPages && TotalPages > 0) pageIndex = TotalPages;

            CurrentPage = pageIndex;

            PagedNews = sortedActive.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
