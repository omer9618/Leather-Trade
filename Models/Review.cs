using System;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string BuyerId { get; set; }
        public virtual ApplicationUser Buyer { get; set; }

        [Required]
        public string SellerId { get; set; }
        public virtual ApplicationUser Seller { get; set; }

        [Required]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; } = false; // To ensure only buyers who completed orders can review
    }
} 