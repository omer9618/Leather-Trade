using System.ComponentModel.DataAnnotations;
using System;

namespace LTMS.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [PastOrTodayDate(ErrorMessage = "Submitted date cannot be in the future.")]
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public int DeliveryTime { get; set; } // Estimated delivery time in days
        public string? ImagePath { get; set; } // Path to uploaded bid image

        // Relationships
        public int DemandId { get; set; }
        public virtual Demand? Demand { get; set; }  // Make nullable

        public string? SellerId { get; set; }  // Make nullable
        public virtual ApplicationUser? Seller { get; set; }  // Make nullable
    }
}