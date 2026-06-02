using System;
using System.ComponentModel.DataAnnotations;

namespace LTMS.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        [Required]
        public int BidId { get; set; }
        public virtual Bid Bid { get; set; }
        
        [Required]
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        
        [Required]
        public string ReceiverId { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        public string? ImagePath { get; set; } // Path to uploaded chat image
    }
} 