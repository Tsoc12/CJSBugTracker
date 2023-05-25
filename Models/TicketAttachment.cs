using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJSBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

       
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? Description { get; set; }

     
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public int TicketId { get; set; }
        [Required]
        public string? BTUserId { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }

        public string? ImageFileType { get; set; }

        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? BTUser { get; set; }
    }
}
