using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;
using System.Security.Claims;

namespace IsisStore.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly AsosContext _context;

        public CheckoutModel(AsosContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Order Order { get; set; }

        public IList<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            
            string cartId = Request.Cookies["IsisCartId"];
            if (string.IsNullOrEmpty(cartId)) return RedirectToPage("/Index");

            CartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartID == cartId)
                .ToListAsync();

            if (!CartItems.Any()) return RedirectToPage("/Index");

            
            TotalPrice = CartItems.Sum(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);

            
            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst("UserID")?.Value;
                if (int.TryParse(userIdString, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        Order = new Order
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email
                        };
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            
            ModelState.Remove("Order.UserID");
            ModelState.Remove("Order.TotalAmount");
            ModelState.Remove("Order.OrderItems");

            if (!ModelState.IsValid)
            {
                
                string cId = Request.Cookies["IsisCartId"];
                CartItems = await _context.CartItems.Include(c => c.Product).Where(c => c.CartID == cId).ToListAsync();
                TotalPrice = CartItems.Sum(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);
                return Page();
            }

            
            string cartId = Request.Cookies["IsisCartId"];
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CartID == cartId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToPage("/Index");

            
            Order.OrderDate = DateTime.Now;
            Order.TotalAmount = cartItems.Sum(item => (item.Product.DiscountPrice ?? item.Product.Price) * item.Quantity);

            
            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst("UserID")?.Value;
                if (int.TryParse(userIdString, out int uid))
                {
                    Order.UserID = uid;
                }
            }

            _context.Orders.Add(Order);
            await _context.SaveChangesAsync(); 

            
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderID = Order.OrderID,
                    ProductID = item.ProductID,
                    SizeName = item.SizeName,
                    Quantity = item.Quantity,
                    Price = item.Product.DiscountPrice ?? item.Product.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Profile");
        }
    }
}