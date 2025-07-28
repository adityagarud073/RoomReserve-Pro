using HotelBookingSystem.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers & Views
builder.Services.AddControllersWithViews();

//  Configure MySQL database connection
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DBS"),
        new MySqlServerVersion(new Version(8, 0, 41))
    )
);

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Add Session Services
builder.Services.AddDistributedMemoryCache(); //  REQUIRED for Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30  Timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Back Button Cache stop karnysaathi  Middleware**
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";
    await next();  // Ensure Middleware Executes Properly
});

//Unauthorized Access Protection Middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path.ToString().ToLower();

    // Ensure Session is Configured Properly BEFORE Accessing
    var session = context.Features.Get<ISessionFeature>()?.Session;
    if (session == null)
    {
        await next();
        return;
    }

    // **Customer/Admin Pages 
    if (path.Contains("customer") || path.Contains("admin"))
    {
        var userEmail = session.GetString("UserEmail");  //  Get Session Properly
        if (string.IsNullOrEmpty(userEmail))
        {
            context.Response.Redirect("/Account/Login");
            return;
        }
    }

    await next(); //  Ensure Middleware Chain Continues
});

// Middleware setup
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅  UseSession() AFTER Routing & BEFORE Authorization
app.UseSession(); //  REQUIRED Middleware
app.UseAuthentication();
app.UseAuthorization();

// Define routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Room}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=BookingRequests}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "customer",
    pattern: "Customer/{action=AvailableRooms}/{id?}",
    defaults: new { controller = "Customer" });

app.Run();
