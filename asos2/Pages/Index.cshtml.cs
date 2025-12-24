using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;
using System.Linq;

namespace IsisStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AsosContext _context;

        public IndexModel(AsosContext context)
        {
            _context = context;
        }

        public IList<Product> Products { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 12;

        public async Task OnGetAsync()
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(p => p.Category == Category);
            }

            var allProducts = await query.ToListAsync();

            List<Product> filteredProducts = allProducts;

            if (!string.IsNullOrEmpty(SearchString))
            {
                string searchTerm = SearchString.ToLower().Trim();

                filteredProducts = allProducts
                    .Select(p => new
                    {
                        Product = p,
                        Score = CalculateRelevanceScore(p, searchTerm)
                    })
                    .Where(x => x.Score > 0) 
                    .OrderByDescending(x => x.Score) 
                    .Select(x => x.Product)
                    .ToList();
            }

            int totalItems = filteredProducts.Count;
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            Products = filteredProducts
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            string cartId = Request.Cookies["IsisCartId"];
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                Response.Cookies.Append("IsisCartId", cartId, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartID == cartId && c.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartID = cartId,
                    ProductID = productId,
                    Quantity = 1,
                    SizeName = "M" 
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { Category = Category, SearchString = SearchString, CurrentPage = CurrentPage });
        }

        public string GetWebImage(string localPath)
        {
            if (string.IsNullOrEmpty(localPath)) return "/images/placeholder.jpg";
            if (localPath.StartsWith("/")) return localPath;
            int index = localPath.IndexOf("images");
            if (index != -1) return "/" + localPath.Substring(index).Replace("\\", "/");
            return localPath;
        }


        private int CalculateRelevanceScore(Product p, string searchTerm)
        {
            int score = 0;
            string name = p.Name?.ToLower() ?? "";
            string desc = p.Description?.ToLower() ?? "";
            string cat = p.Category?.ToLower() ?? "";

            if (name == searchTerm) score += 100;

            else if (name.Contains(searchTerm)) score += 50;

            if (cat.Contains(searchTerm)) score += 30;

            if (desc.Contains(searchTerm)) score += 10;

            var searchWords = searchTerm.Split(' ');
            var productWords = (name + " " + cat).Split(' ');

            foreach (var sWord in searchWords)
            {
                foreach (var pWord in productWords)
                {
                    if (ComputeLevenshteinDistance(sWord, pWord) <= 2)
                    {
                        score += 15;
                    }
                }
            }

            return score;
        }

        private static int ComputeLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}