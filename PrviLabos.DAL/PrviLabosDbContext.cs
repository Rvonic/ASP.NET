using Microsoft.EntityFrameworkCore;
using PrviLabos.Model;

namespace PrviLabos.DAL;

public class PrviLabosDbContext : DbContext
{
    public PrviLabosDbContext(DbContextOptions<PrviLabosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SupportAgent> Agents => Set<SupportAgent>();
    public DbSet<SupportTicket> Tickets => Set<SupportTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.DailyRate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Vehicle)
            .WithMany(v => v.Bookings)
            .HasForeignKey(b => b.VehicleId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.PickupLocation)
            .WithMany()
            .HasForeignKey(b => b.PickupLocationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.PlannedDropoffLocation)
            .WithMany()
            .HasForeignKey(b => b.PlannedDropoffLocationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.SupportTickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.RequestedDropoffLocation)
            .WithMany(l => l.SupportTickets)
            .HasForeignKey(t => t.RequestedDropoffLocationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}