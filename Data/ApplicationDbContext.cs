using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LTMS.Models; // Add this namespace for ApplicationUser

namespace LTMS.Data // Should match your folder structure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSets for your custom entities here if needed
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}