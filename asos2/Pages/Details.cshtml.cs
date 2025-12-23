using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IsisStore.Data;
using IsisStore.Models;
using Microsoft.EntityFrameworkCore;

namespace IsisStore.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly AsosContext _context;

        public DetailsModel(AsosContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Product = await _context.Products.FirstOrDefaultAsync(m => m.ProductID == id);

            if (Product == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
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

            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.CartID == cartId && c.ProductID == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity; 
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartID = cartId,
                    ProductID = productId,
                    Quantity = quantity, 
                    SizeName = "M"
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("./Cart");
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