using System;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public string Type { get; set; }
        
        public bool IsRead { get; set; }
        
        public string? ReferenceId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ApplicationUser? User { get; set; }
    }
} 