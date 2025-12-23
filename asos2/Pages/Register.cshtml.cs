using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IsisStore.Data;
using IsisStore.Models;

namespace IsisStore.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AsosContext _context;

        public RegisterModel(AsosContext context) { _context = context; }

        [BindProperty]
        public User Input { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            
            ModelState.Remove("Input.UserID");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_context.Users.Any(u => u.Email == Input.Email))
            {
                ErrorMessage = "Email is already taken.";
                return Page();
            }

            Input.CreatedAt = DateTime.Now;

            _context.Users.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }
    }
}