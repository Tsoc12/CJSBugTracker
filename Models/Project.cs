using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJSBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? Name { get; set;}

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? Description { get; set;}
        
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        public bool Archived { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual BTUser? Member { get; set; }
        public virtual Ticket? Ticket { get; set; }
    }
}
