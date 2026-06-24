using Microsoft.EntityFrameworkCore;
using RentingBooking.Enum;
using RentingBooking.Models;

namespace RentingBooking.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync() && await context.Properties.AnyAsync())
            return;

        // If partial seed exists, clear everything and re-seed
        if (await context.Users.AnyAsync() || await context.Properties.AnyAsync() ||
            await context.Bookings.AnyAsync() || await context.WishlistItems.AnyAsync())
        {
            context.NotificationLogs.RemoveRange(context.NotificationLogs);
            context.WishlistItems.RemoveRange(context.WishlistItems);
            context.Bookings.RemoveRange(context.Bookings);
            context.PropertyImages.RemoveRange(context.PropertyImages);
            context.Properties.RemoveRange(context.Properties);
            context.KycVerifications.RemoveRange(context.KycVerifications);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }

        // ── Users ────────────────────────────────────────────────────────
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ownerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var customerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var customer2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var users = new List<User>
        {
            new()
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@rentingbooking.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = ownerId,
                Username = "owner",
                Email = "owner@rentingbooking.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner123!"),
                Role = UserRole.Owner,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = customerId,
                Username = "customer",
                Email = "customer@rentingbooking.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = customer2Id,
                Username = "juan",
                Email = "juan@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // ── KYC ──────────────────────────────────────────────────────────
        context.KycVerifications.AddRange(new List<KycVerification>
        {
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                UserId = customerId,
                DocumentNumber = "DNI12345678",
                FirstName = "Carlos",
                LastName = "García",
                BirthDate = new DateTime(1990, 5, 15),
                Status = KycStatus.Accepted,
                VerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                UserId = customer2Id,
                DocumentNumber = "DNI87654321",
                FirstName = "Juan",
                LastName = "Pérez",
                BirthDate = new DateTime(1985, 8, 22),
                Status = KycStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        });
        await context.SaveChangesAsync();

        // ── Properties ───────────────────────────────────────────────────
        var p1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var p2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var p3 = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var p4 = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var p5 = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

        var properties = new List<Property>
        {
            new()
            {
                Id = p1, HostId = ownerId,
                Title = "Villa Serenidad Beachfront",
                Description = "Hermosa villa frente al mar con piscina infinita, jardines tropicales y acceso directo a la playa. Perfecta para familias o grupos grandes.",
                Location = "Tulum, Quintana Roo, México",
                PricePerNight = 250.00m, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = p2, HostId = ownerId,
                Title = "Mountain Retreat Cabin",
                Description = "Acogedora cabaña de montaña con chimenea, jacuzzi exterior y vistas panorámicas. Ideal para escapadas románticas.",
                Location = "Valle de Bravo, Estado de México",
                PricePerNight = 180.00m, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = p3, HostId = ownerId,
                Title = "Urban Loft Downtown",
                Description = "Loft moderno en el corazón de la ciudad. Cerca de restaurantes, museos y vida nocturna. Perfecto para viajeros de negocios.",
                Location = "Ciudad de México, CDMX",
                PricePerNight = 120.00m, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = p4, HostId = ownerId,
                Title = "Casa del Sol",
                Description = "Encantadora casa colonial restaurada con patio interior, fuente y azotea con vista a la catedral.",
                Location = "San Miguel de Allende, Guanajuato",
                PricePerNight = 150.00m, IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = p5, HostId = ownerId,
                Title = "Modern Studio",
                Description = "Estudio minimalista completamente equipado con cocina abierta y balcón. Excelente conectividad WiFi.",
                Location = "Guadalajara, Jalisco",
                PricePerNight = 85.00m, IsActive = true, CreatedAt = DateTime.UtcNow
            }
        };

        context.Properties.AddRange(properties);
        await context.SaveChangesAsync();

        // ── PropertyImages ───────────────────────────────────────────────
        context.PropertyImages.AddRange(new List<PropertyImage>
        {
            new() { Id = Guid.Parse("af000001-0001-0001-0001-000000000001"), PropertyId = p1, Url = "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?auto=format&fit=crop&w=800&q=80", Order = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000001-0001-0001-0001-000000000002"), PropertyId = p1, Url = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=800&q=80", Order = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000001-0001-0001-0001-000000000003"), PropertyId = p1, Url = "https://images.unsplash.com/photo-1522771739017-7c1?villa&w=800&q=80", Order = 3, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000002-0002-0002-0002-000000000001"), PropertyId = p2, Url = "https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?auto=format&fit=crop&w=800&q=80", Order = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000002-0002-0002-0002-000000000002"), PropertyId = p2, Url = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?auto=format&fit=crop&w=800&q=80", Order = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000003-0003-0003-0003-000000000001"), PropertyId = p3, Url = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80", Order = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000003-0003-0003-0003-000000000002"), PropertyId = p3, Url = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=800&q=80", Order = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000004-0004-0004-0004-000000000001"), PropertyId = p4, Url = "https://images.unsplash.com/photo-1583608205776-bfd35f0d9f83?auto=format&fit=crop&w=800&q=80", Order = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000004-0004-0004-0004-000000000002"), PropertyId = p4, Url = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80", Order = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.Parse("af000005-0005-0005-0005-000000000001"), PropertyId = p5, Url = "https://images.unsplash.com/photo-1536376072261-38c75010e6c9?auto=format&fit=crop&w=800&q=80", Order = 1, CreatedAt = DateTime.UtcNow }
        });
        await context.SaveChangesAsync();

        // ── Bookings ─────────────────────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.Today);
        context.Bookings.AddRange(new List<Booking>
        {
            new()
            {
                Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"),
                PropertyId = p1, GuestId = customerId,
                CheckInDate = today.AddDays(-10), CheckOutDate = today.AddDays(-5),
                PricePerNightAtBooking = 250.00m,
                TotalPrice = 250.00m * 5,
                Status = BookingStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                Id = Guid.Parse("f0000002-0000-0000-0000-000000000002"),
                PropertyId = p2, GuestId = customerId,
                CheckInDate = today.AddDays(-3), CheckOutDate = today.AddDays(2),
                PricePerNightAtBooking = 180.00m,
                TotalPrice = 180.00m * 5,
                Status = BookingStatus.Paid,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("f0000003-0000-0000-0000-000000000003"),
                PropertyId = p3, GuestId = customer2Id,
                CheckInDate = today.AddDays(7), CheckOutDate = today.AddDays(10),
                PricePerNightAtBooking = 120.00m,
                TotalPrice = 120.00m * 3,
                Status = BookingStatus.PendingPayment,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("f0000004-0000-0000-0000-000000000004"),
                PropertyId = p1, GuestId = customer2Id,
                CheckInDate = today.AddDays(20), CheckOutDate = today.AddDays(25),
                PricePerNightAtBooking = 250.00m,
                TotalPrice = 250.00m * 5,
                Status = BookingStatus.PendingPayment,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("f0000005-0000-0000-0000-000000000005"),
                PropertyId = p4, GuestId = customerId,
                CheckInDate = today.AddDays(-20), CheckOutDate = today.AddDays(-15),
                PricePerNightAtBooking = 150.00m,
                TotalPrice = 150.00m * 5,
                Status = BookingStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                Id = Guid.Parse("f0000006-0000-0000-0000-000000000006"),
                PropertyId = p5, GuestId = customerId,
                CheckInDate = today.AddDays(-7), CheckOutDate = today.AddDays(-3),
                PricePerNightAtBooking = 85.00m,
                TotalPrice = 85.00m * 4,
                Status = BookingStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                Id = Guid.Parse("f0000007-0000-0000-0000-000000000007"),
                PropertyId = p2, GuestId = customer2Id,
                CheckInDate = today.AddDays(-15), CheckOutDate = today.AddDays(-10),
                PricePerNightAtBooking = 180.00m,
                TotalPrice = 180.00m * 5,
                Status = BookingStatus.Canceled,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                Id = Guid.Parse("f0000008-0000-0000-0000-000000000008"),
                PropertyId = p3, GuestId = customerId,
                CheckInDate = today.AddDays(14), CheckOutDate = today.AddDays(18),
                PricePerNightAtBooking = 120.00m,
                TotalPrice = 120.00m * 4,
                Status = BookingStatus.Paid,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        });
        await context.SaveChangesAsync();

        // ── WishlistItems ────────────────────────────────────────────────
        context.WishlistItems.AddRange(new List<WishListItem>
        {
            new()
            {
                Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"),
                UserId = customerId, PropertyId = p2,
                AddedAt = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.Parse("f1000002-0000-0000-0000-000000000002"),
                UserId = customerId, PropertyId = p4,
                AddedAt = DateTime.UtcNow.AddDays(-3), CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.Parse("f1000003-0000-0000-0000-000000000003"),
                UserId = customer2Id, PropertyId = p1,
                AddedAt = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.Parse("f1000004-0000-0000-0000-000000000004"),
                UserId = customer2Id, PropertyId = p5,
                AddedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        });
        await context.SaveChangesAsync();

        // ── NotificationLogs ─────────────────────────────────────────────
        context.NotificationLogs.AddRange(new List<NotificationLog>
        {
            new()
            {
                Id = Guid.Parse("f2000001-0000-0000-0000-000000000001"),
                UserId = customerId,
                Type = NotificationType.Email,
                Event = NotificationEvent.BookingConfirmed,
                Message = "Tu reserva en Villa Serenidad Beachfront ha sido confirmada.",
                IsRead = true,
                SentAt = DateTime.UtcNow.AddDays(-15),
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                Id = Guid.Parse("f2000002-0000-0000-0000-000000000002"),
                UserId = ownerId,
                Type = NotificationType.InApp,
                Event = NotificationEvent.CheckInReminder,
                Message = "Recordatorio: Tu huésped Carlos llega mañana a Mountain Retreat Cabin.",
                IsRead = false,
                SentAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("f2000003-0000-0000-0000-000000000003"),
                UserId = customerId,
                Type = NotificationType.Email,
                Event = NotificationEvent.KycApproved,
                Message = "Tu verificación KYC ha sido aprobada. Ya puedes reservar propiedades.",
                IsRead = false,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("f2000004-0000-0000-0000-000000000004"),
                UserId = customerId,
                Type = NotificationType.InApp,
                Event = NotificationEvent.CheckOutReminder,
                Message = "Recordatorio: Tu check-out en Villa Serenidad Beachfront es mañana.",
                IsRead = false,
                SentAt = DateTime.UtcNow.AddDays(-6),
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            }
        });
        await context.SaveChangesAsync();
    }
}
