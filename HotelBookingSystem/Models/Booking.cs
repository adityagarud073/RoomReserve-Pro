using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        [Required]
        public int CustomerId { get; set; } // Assuming each booking is linked to a Customer

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public bool IsRejected { get; set; } = false; // Default = Not rejected
        public bool IsConfirmed { get; set; } = false; // Default =  Not confirmed
    }
}
