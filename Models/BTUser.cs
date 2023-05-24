using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Azure;

namespace CJSBugTracker.Models
{
    public class BTUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? LastName { get; set; }


        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        //Image properties
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }



        public virtual Company? Company { get; set; }
       


        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

    }
}
