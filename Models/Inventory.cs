using System;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        
        [Required]
        public string ProductName { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string UnitOfMeasurement { get; set; } = "Square Feet"; // Default unit
        
        [Required]
        public int LeatherTypeId { get; set; }
        public virtual LeatherType? LeatherType { get; set; }
        
        public string Status { get; set; } = "Available";  // Available, Low Stock, Out of Stock
        
        [Required]
        [DataType(DataType.DateTime)]
        [PastOrTodayDate(ErrorMessage = "Last updated date cannot be in the future.")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public string? ImagePath { get; set; } // Path to the uploaded image

        // Relationships
        public string? SellerId { get; set; }
        public virtual ApplicationUser? Seller { get; set; }
    }
} 