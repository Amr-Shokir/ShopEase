using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;
using Microsoft.AspNetCore.Authorization;

namespace IsisStore.Pages
{
    [Authorize]
    public class CartModel : PageModel
    {
        private readonly AsosContext _context;

        public CartModel(AsosContext context)
        {
            _context = context;
        }

        public IList<CartItem> CartItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Shipping { get; set; } = 10.00m; // Example flat rate
        public decimal TotalPrice { get; set; }

        public async Task OnGetAsync()
        {
            string cartId = Request.Cookies["IsisCartId"];

            if (!string.IsNullOrEmpty(cartId))
            {
                CartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.CartID == cartId)
                    .ToListAsync();

                SubTotal = CartItems.Sum(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);

                Shipping = CartItems.Any() ? 10.00m : 0m;

                TotalPrice = SubTotal + Shipping;
            }
            else
            {
                CartItems = new List<CartItem>();
            }
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int id, int change)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                item.Quantity += change;

                if (item.Quantity < 1) item.Quantity = 1;


                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
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