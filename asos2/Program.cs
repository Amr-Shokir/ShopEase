using IsisStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AsosContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";   // Redirect here if not logged in
        options.LogoutPath = "/Logout"; // Redirect here upon logout
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// *** CRITICAL: Authentication must come BEFORE Authorization ***
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
app.Run();