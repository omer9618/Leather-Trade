using System;
using System.ComponentModel.DataAnnotations;
using LTMS.Models;

namespace LTMS.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public string BuyerId { get; set; }
        
        [Required]
        public string SellerId { get; set; }
        
        [Required]
        public int BidId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        [PastOrTodayDate(ErrorMessage = "Order date cannot be in the future.")]
        public DateTime OrderDate { get; set; }
        
        [Required]
        public string Status { get; set; } // Pending, Completed, Cancelled
        
        // Navigation properties
        public ApplicationUser Buyer { get; set; }
        public ApplicationUser Seller { get; set; }
        public Bid Bid { get; set; }
        public Payment Payment { get; set; }
    }
} 