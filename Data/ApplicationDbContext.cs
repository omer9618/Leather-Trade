using LTMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LTMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Demand> Demands { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<LeatherType> LeatherTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Bid>()
                .HasOne(b => b.Demand)
                .WithMany(d => d.Bids)
                .HasForeignKey(b => b.DemandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bid>()
                .HasOne(b => b.Seller)
                .WithMany()
                .HasForeignKey(b => b.SellerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Demand>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Inventory>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            builder.Entity<Inventory>()
                .HasOne(i => i.Seller)
                .WithMany()
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Entity<Order>()
                .HasOne(o => o.Bid)
                .WithMany()
                .HasForeignKey(o => o.BidId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Payment>()
                .HasOne(p => p.Bid)
                .WithMany()
                .HasForeignKey(p => p.BidId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Buyer)
                .WithMany()
                .HasForeignKey(p => p.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.Buyer)
                .WithMany()
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatMessage relationships
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Bid)
                .WithMany()
                .HasForeignKey(m => m.BidId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default leather types
            builder.Entity<LeatherType>().HasData(LeatherType.GetDefaultTypes());

            // Robustness: Add required fields and check constraints
            builder.Entity<Demand>()
                .Property(d => d.Title)
                .IsRequired();
            builder.Entity<Demand>()
                .Property(d => d.Description)
                .IsRequired();
            builder.Entity<Demand>()
                .Property(d => d.Status)
                .IsRequired();
            builder.Entity<Demand>()
                .Property(d => d.CreatedDate)
                .IsRequired();
            builder.Entity<Demand>()
                .Property(d => d.Deadline)
                .IsRequired();
            builder.Entity<Demand>()
                .HasIndex(d => d.Title);

            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2)
                .IsRequired();
            builder.Entity<Bid>()
                .Property(b => b.DeliveryTime)
                .IsRequired();
            builder.Entity<Bid>()
                .Property(b => b.Status)
                .IsRequired();
            builder.Entity<Bid>()
                .HasIndex(b => b.Amount);
            builder.Entity<Bid>()
                .HasIndex(b => new { b.SellerId, b.DemandId }).IsUnique(); // Prevent duplicate bids by same seller on same demand

            builder.Entity<Inventory>()
                .Property(i => i.ProductName)
                .IsRequired();
            builder.Entity<Inventory>()
                .Property(i => i.Quantity)
                .IsRequired();
            builder.Entity<Inventory>()
                .Property(i => i.Price)
                .HasPrecision(18, 2)
                .IsRequired();
            builder.Entity<Inventory>()
                .HasIndex(i => i.ProductName);

            builder.Entity<Order>()
                .Property(o => o.OrderDate)
                .IsRequired();
            builder.Entity<Order>()
                .Property(o => o.Status)
                .IsRequired();
            builder.Entity<Order>()
                .HasIndex(o => o.OrderDate);
        }
    }
}