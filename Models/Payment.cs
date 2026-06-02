using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTMS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        
        [Required]
        public int BidId { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();

        // Additional properties for Stripe integration
        [StringLength(100)]
        public string? StripePaymentIntentId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string BuyerId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string SellerId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Bid? Bid { get; set; }
        public Order? Order { get; set; }
        public ApplicationUser? Buyer { get; set; }
        public ApplicationUser? Seller { get; set; }
    }
} 