using System.ComponentModel.DataAnnotations;

namespace CJSBugTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
