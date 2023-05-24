using System.ComponentModel.DataAnnotations;

namespace CJSBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Display(Name = "Comment")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        [Required]
        public string? Comment { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        public int TicketId { get; set; }
        public string? UserId { get; set; }

        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }

        
    }
}
