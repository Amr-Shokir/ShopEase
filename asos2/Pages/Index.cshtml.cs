using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;

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

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(p => p.Name.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(p => p.Category == Category);
            }

            int totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;

            Products = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId)
        {
            // 1. Force Login Check
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
    }
}