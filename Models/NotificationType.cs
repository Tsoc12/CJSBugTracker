using System.ComponentModel.DataAnnotations;

namespace CJSBugTracker.Models
{
    public class NotificationType
    {
        [Required]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters longs.", MinimumLength = 2)]
        public int Id { get; set; }

       public string? Name { get; set; }
    }
}
