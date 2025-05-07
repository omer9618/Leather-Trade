using Microsoft.AspNetCore.Identity;

namespace LTMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }  // Custom field
        
    }
}
