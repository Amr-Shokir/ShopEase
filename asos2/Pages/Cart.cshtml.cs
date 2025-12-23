using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;
using Microsoft.AspNetCore.Authorization;

namespace IsisStore.Pages
{
    [Authorize] // Forces the user to be logged in to access this page
    public class CartModel : PageModel
    {
        private readonly AsosContext _context;

        public CartModel(AsosContext context)
        {
            _context = context;
        }

        public IList<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }

        public async Task OnGetAsync()
        {
            // Retrieve the cart ID from the cookie
            string cartId = Request.Cookies["IsisCartId"];

            if (!string.IsNullOrEmpty(cartId))
            {
                // Fetch items for this cart ID
                CartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.CartID == cartId)
                    .ToListAsync();

                // Calculate the total price based on DiscountPrice or regular Price
                TotalPrice = CartItems.Sum(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);
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

        // Helper method to resolve image paths
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