using HotelBookingSystem.Data;
using HotelBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace HotelBookingSystem.Controllers
{
    public class RoomController : Controller
    {
        private readonly HotelDbContext _context;

        public RoomController(HotelDbContext context)
        {
            _context = context;
        }

        // Existing AddRoom Feature (As it is)
        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpPost]
       
        public async Task<IActionResult> AddRoom(Room room, IFormFile imageFile)
        {
            //if (model.ImageFile != null)
            //{
            //    using (var ms = new MemoryStream())
            //    {
            //        await model.ImageFile.CopyToAsync(ms);
            //        model.Image = ms.ToArray(); // Convert Image to Byte Array
            //    }
            //}

            //_context.Rooms.Add(model);
            //await _context.SaveChangesAsync();

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            room.AdminId = adminId; // **AdminId सेट करा**

            if (imageFile != null && imageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    room.Image = memoryStream.ToArray();
                }
            }

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

         

            //return RedirectToAction("AdminDashboard");

            return RedirectToAction("RoomList");


        }

        // Room List(Customer & Admin)
        public async Task<IActionResult> RoomList()
        {
            //var userId = HttpContext.Session.GetInt32("UserId");
            //var userRole = HttpContext.Session.GetString("UserRole");

            //var rooms = await _context.Rooms.ToListAsync();
            //return View(rooms);

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

        
            var rooms = await _context.Rooms
                .Where(r => r.AdminId == adminId)
                .ToListAsync();


            return View(rooms);


        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBookRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null || room.IsBooked)
                return NotFound();

            room.IsBooked = true;
            _context.Update(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(RoomList));
        }

        // Confirm Delete Page
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        // Confirm Delete
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            //After delete redirect to RoomList page
            return RedirectToAction("RoomList");
        }

        public async Task<IActionResult> RoomDetails(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Ratings) //  Ratings 
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
                return NotFound();

            return View(room);
        }

        public async Task<IActionResult> FrontRoom(int id)
        {
            var room = await _context.Rooms
             
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
                return NotFound();

            return View(room);
        }


        // Rating (Only Customers)
        [HttpPost]
        public IActionResult AddRating(int roomId, int stars, string customerName, string review)
        {
            if (stars < 1 || stars > 5)
            {
                return BadRequest("Invalid Rating. It should be between 1 and 5.");
            }

            var rating = new Rating
            {
                RoomId = roomId,
                Stars = stars,
                CustomerName = customerName,
                Review = review
            };

            _context.Ratings.Add(rating);
            _context.SaveChanges();

            return RedirectToAction("RoomDetails", new { roomId = roomId });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRating(int roomId, string customerName, int stars, string review)
        {
            if (stars < 1 || stars > 5)
            {
                return BadRequest("Invalid rating. Must be between 1 and 5 stars.");
            }

            var rating = new Rating
            {
                RoomId = roomId,
                CustomerName = customerName,
                Stars = stars,
                Review = review
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("RoomDetails", new { id = roomId });
        }

           public IActionResult Index()
        {
            var availableRooms = _context.Rooms
                                         .Where(r => !r.IsBooked) // only Available Rooms 
                                         .ToList();

            return View(availableRooms);
        }

    }


}

