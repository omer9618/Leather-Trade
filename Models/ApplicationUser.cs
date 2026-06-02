using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            FullName = string.Empty;
        }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; }
    }
}