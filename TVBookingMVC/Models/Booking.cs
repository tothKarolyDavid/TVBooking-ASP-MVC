using System.ComponentModel.DataAnnotations;

namespace TVBookingMVC.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Program { get; set; }

        [Required]
        public string? Channel { get; set; }

        [Required]
        public string? Genre { get; set; }

        [Required]
        [Display(Name = "Start time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Start { get; set; }

        [Required]
        [Display(Name = "End time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime End { get; set; }

        [Required]
        [Display(Name = "Age limit")]
        public string? AgeLimit { get; set; }

        [Required]
        [Display(Name = "Room number")]
        [Range(1, 999)]
        public int RoomNumber { get; set; }
    }
}
