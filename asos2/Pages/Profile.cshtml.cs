using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IsisStore.Data;
using IsisStore.Models;
using System.Security.Claims;

namespace IsisStore.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly AsosContext _context;

        public ProfileModel(AsosContext context) { _context = context; }

        public IsisStore.Models.User CurrentUser { get; set; }

        public List<Order> OrderHistory { get; set; }

        public async Task OnGetAsync()
        {
            var userIdString = User.FindFirst("UserID")?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                CurrentUser = await _context.Users.FindAsync(userId);

                OrderHistory = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserID == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
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