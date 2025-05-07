using Microsoft.AspNetCore.Identity;

namespace LTMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }  // Changed from required
   
    }
}