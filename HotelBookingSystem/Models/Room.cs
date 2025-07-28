using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingSystem.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomName { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Rent { get; set; } // Rent per day

        public string Location { get; set; }

        public bool IsAvailable { get; set; } = true; // Default available

        public bool IsBooked { get; set; } = false; // Default not booked

        public byte[] Image { get; set; } // Store image as Byte Array

        [NotMapped] // Prevent storing in DB
        public IFormFile ImageFile { get; set; } // For file upload

        public List<Rating> Ratings { get; set; } = new List<Rating>();

        public int? AdminId { get; set; } //  Nullable AdminId

        [ForeignKey("AdminId")]
        public User Admin { get; set; }
    }
}
