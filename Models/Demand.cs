using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System;

namespace LTMS.Models
{
    public class Demand
    {
        public Demand()
        {
            Bids = new List<Bid>();
            Title = string.Empty;
            Description = string.Empty;
            Status = "Open";
            CreatedDate = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters")]
        public string Description { get; set; }

        [StringLength(1000, ErrorMessage = "Requirements cannot be longer than 1000 characters")]
        public string? Requirements { get; set; }

        [Required(ErrorMessage = "Leather type is required")]
        public int LeatherTypeId { get; set; }
        public virtual LeatherType? LeatherType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit of measurement is required")]
        public string UnitOfMeasurement { get; set; } = "Square Feet"; // Default unit

        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Deadline must be a future date.")]
        public DateTime Deadline { get; set; }

        public string Status { get; set; }

        // Relationships
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Bid> Bids { get; set; }
    }
}