using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotelBookingSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HotelDbContext _context;

        public CustomerController(HotelDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> AvailableRooms()
        {
            var rooms = await _context.Rooms.ToListAsync(); 
            return View(rooms);
        }

        //  Show Confirm Booking Page
        //public IActionResult BookNow()
        //{
      
        //    return View();
        //}


        public async Task<IActionResult> BookRoom(int roomId)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = new Booking
            {
                CustomerId = customerId.Value, 
                RoomId = roomId,
                BookingDate = DateTime.Now,
                IsConfirmed = false,
                IsRejected = false
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // After book right page open

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null)
            {
                return NotFound();
            }

            return View("BookRoom", room); 
        }



        [HttpPost]
        public IActionResult ConfirmBooking(int roomId)
        {
            return RedirectToAction("BookingSuccess");
        }


        //  Booking Success Page
        public IActionResult BookingSuccess()
        {
            return View();
        }
        public async Task<IActionResult> RoomDetails(int id)
        {
            // Fetch Room Details by ID
            var room = await _context.Rooms
                .Include(r => r.Ratings) 
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        public async Task<IActionResult> MyBookings()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // particular customer request
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();

            Console.WriteLine($"Total Booking Requests for Customer {customerId}: {bookings.Count}");

            return View(bookings);
        }



        [HttpPost]
        public async Task<IActionResult> AddRating(int RoomId, int Stars, string? Review)
        {
            var rating = new Rating
            {
                RoomId = RoomId,
                Stars = Stars,
                Review = Review,
                CustomerName = User.Identity.Name // Logged-in user
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("RoomDetails", new { id = RoomId });
        }

        public async Task<IActionResult> SearchRooms(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return RedirectToAction("AvailableRooms"); 
            }

            var rooms = await _context.Rooms
                .Where(r => r.Location.Contains(location))
                .ToListAsync();

            if (!rooms.Any())
            {
                ViewBag.Message = "❌ No rooms found in this location.";
            }

            return View("RoomList", rooms); 
        }


    }
}


