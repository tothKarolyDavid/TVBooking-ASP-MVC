using System.ComponentModel.DataAnnotations;

namespace TVBookingMVC.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Program { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Channel { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Genre { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start time")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Start { get; set; }

        [Required]
        [Display(Name = "End time")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime End { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Age limit")]
        public string AgeLimit { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Room number")]
        [Range(0, 999)]
        public int RoomNumber { get; set; }
    }
}
