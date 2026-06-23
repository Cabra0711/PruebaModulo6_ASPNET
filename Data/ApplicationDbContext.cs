using Microsoft.EntityFrameworkCore;
using RentingBooking.Models;

namespace RentingBooking.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<KycVerification> KycVerifications => Set<KycVerification>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WishListItem> WishlistItems => Set<WishListItem>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
            e.Ignore(u => u.Token);
        });

        // ── KycVerification — 1:1 con User ────────────────────────────────────
        modelBuilder.Entity<KycVerification>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.Status).HasConversion<string>();
            e.Property(k => k.DocumentNumber).IsRequired().HasMaxLength(50);
            e.Property(k => k.FirstName).IsRequired().HasMaxLength(100);
            e.Property(k => k.LastName).IsRequired().HasMaxLength(100);

            e.HasOne(k => k.User)
             .WithOne(u => u.KycVerification)
             .HasForeignKey<KycVerification>(k => k.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Property ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Property>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).IsRequired().HasMaxLength(200);
            e.Property(p => p.Location).IsRequired().HasMaxLength(300);
            e.Property(p => p.PricePerNight).HasColumnType("decimal(18,2)");

            // Optimistic concurrency (ya lo tenías — se mantiene)
            e.Property(p => p.RowVersion).IsRowVersion();

            // Índice para búsqueda pública por ubicación
            e.HasIndex(p => new { p.Location, p.IsActive });

            // Owner → Properties  (1:N)
            e.HasOne(p => p.Host)
             .WithMany(u => u.Properties)
             .HasForeignKey(p => p.HostId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PropertyImage — 1:N con Property ─────────────────────────────────
        modelBuilder.Entity<PropertyImage>(e =>
        {
            e.HasKey(pi => pi.Id);
            e.Property(pi => pi.Url).IsRequired().HasMaxLength(500);

            e.HasOne(pi => pi.Property)
             .WithMany(p => p.Images)
             .HasForeignKey(pi => pi.PropertyId)
             .OnDelete(DeleteBehavior.Cascade); // si se borra el inmueble, se borran las fotos
        });

        // ── Booking ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            e.Property(b => b.PricePerNightAtBooking).HasColumnType("decimal(18,2)");
            e.Property(b => b.Status).HasConversion<string>();

            // Índice para consultas de disponibilidad y dashboard
            e.HasIndex(b => new { b.PropertyId, b.CheckInDate, b.CheckOutDate })
             .HasDatabaseName("IX_Booking_Property_Dates");
            e.HasIndex(b => new { b.PropertyId, b.Status });

            // Booking → Property  (N:1)
            e.HasOne(b => b.Property)
             .WithMany(p => p.Bookings)
             .HasForeignKey(b => b.PropertyId)
             .OnDelete(DeleteBehavior.Restrict);

            // Booking → Guest/User  (N:1)
            e.HasOne(b => b.Guest)
             .WithMany(u => u.Bookings)
             .HasForeignKey(b => b.GuestId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WishListItem — N:M entre User y Property ──────────────────────────
        modelBuilder.Entity<WishListItem>(e =>
        {
            e.HasKey(w => w.Id);

            // Un usuario no puede guardar el mismo inmueble dos veces
            e.HasIndex(w => new { w.UserId, w.PropertyId }).IsUnique();

            e.HasOne(w => w.User)
             .WithMany(u => u.Wishlist)
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(w => w.Property)
             .WithMany()
             .HasForeignKey(w => w.PropertyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── NotificationLog ───────────────────────────────────────────────────
        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Message).IsRequired().HasMaxLength(500);

            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}