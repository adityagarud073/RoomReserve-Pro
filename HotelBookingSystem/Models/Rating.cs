using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBookingSystem.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        public string? CustomerName { get; set; }

        public string? Review { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }
    }
}
