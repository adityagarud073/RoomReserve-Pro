using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;

namespace HotelBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly HotelDbContext _context;

        public AccountController(HotelDbContext context)
        {
            _context = context;
        }

        //  Register Page
        public IActionResult Register()
        {
            return View();
        }

        //  Register User
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            try
            {
                

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    ViewBag.Message = "Full Name is required!";
                    return View();
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ViewBag.Message = "Email already exists!";
                    return View();
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = model.PasswordHash, // Plain password store karto
                    Role = string.IsNullOrEmpty(model.Role) ? "Customer" : model.Role
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Registration Successful!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                //Console.WriteLine( ex.Message);
              
                ViewBag.Message = "Error: " + ex.Message;
                return View();
            }
        }



        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        //  Validate Login 
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // User Fetch from Database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Check if User Exists
            if (user != null)
            {
                //Console.WriteLine({user.PasswordHash});
                //Console.WriteLine({password});

                // Compare Plain Text Passwords
                if (user.PasswordHash == password)
                {
                    // Session Store
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    if (user.Role == "Admin")
                        return RedirectToAction("RoomList", "Room", new { id = user.Id });
                    else
                        return RedirectToAction("AvailableRooms", "Customer", new { id = user.Id });
                }
            }

            ViewBag.Message = "Invalid login credentials!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Session Remove 
            Response.Cookies.Delete(".AspNetCore.Session"); // Session Cookie Delete 
            Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Authentication Cookie Delete
            return RedirectToAction("Login", "Account"); // Login Page वर Redirect
        }

        public IActionResult About()
        {
            return View();
        }

    }
}
