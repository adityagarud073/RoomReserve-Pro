using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace HotelBookingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly HotelDbContext _context;

        public AdminController(HotelDbContext context)
        {
            _context = context;
        }

        //  All Booking Requests
        public async Task<IActionResult> BookingRequests()
        {
            //var requests = await _context.Bookings.Include(b => b.Room).ToListAsync();
            //return View(requests);

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //  Admin Add Rooms Requests Filter 
            var requests = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => _context.Rooms.Any(r => r.RoomId == b.RoomId && r.AdminId == adminId))
                .ToListAsync();

            Console.WriteLine($"Total Booking Requests for Admin {adminId}: {requests.Count}");

            return View(requests);

        }

        // Confirm Booking
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null)
            {
                room.IsBooked = true;
                _context.Update(room);
            }

            booking.IsConfirmed = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingRequests");
           
        }

        // Reject Booking Confirmation Page
        public async Task<IActionResult> RejectConfirmation(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Room)
                                                 .FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // Actual Reject Booking
        [HttpPost]
        public async Task<IActionResult> RejectBooking(int id)
        {
            //var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == id);
            //if (booking == null) return NotFound();

            //var room = await _context.Rooms.FindAsync(booking.RoomId);
            //if (room != null)
            //{
            //    room.IsBooked = false; // Room available again
            //    _context.Update(room);
            //}

            //booking.IsRejected = true;
            //booking.IsConfirmed = false;

            //_context.Update(booking);
            //await _context.SaveChangesAsync();

            //return RedirectToAction("BookingRequests");

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null || booking.Room.AdminId != adminId)
            {
                return Unauthorized(); // admin only delte own room 
            }

            booking.IsRejected = true;
            booking.IsConfirmed = false;
            booking.Room.IsBooked = false;

            _context.Update(booking);
            _context.Update(booking.Room);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingRequests");
        }
    }
}
