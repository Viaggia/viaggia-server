using Microsoft.EntityFrameworkCore;
using viaggia_server.Models.Addresses;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.Companions;
using viaggia_server.Models.HotelDates;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;

namespace viaggia_server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for all entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<PackageDate> PackageDates { get; set; } = null!;
        public DbSet<Hotel> Hotels { get; set; } = null!;
        public DbSet<HotelRoomType> RoomTypes { get; set; } = null!;
        public DbSet<HotelDate> HotelDates { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<BillingAddress> BillingAddresses { get; set; } = null!;
        public DbSet<Media> Medias { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Companion> Companions { get; set; } = null!;
        public DbSet<Commoditie> Commodities { get; set; }
        public DbSet<CommoditieServices> CommoditieServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH inheritance for Address and BillingAddress
            modelBuilder.Entity<Address>()
                .HasDiscriminator<string>("AddressType")
                .HasValue<Address>("Address")
                .HasValue<BillingAddress>("BillingAddress");

            // Configuration for UserRole (composite key)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Package
            modelBuilder.Entity<Package>()
                .HasMany(p => p.PackageDates)
                .WithOne(pd => pd.Package)
                .HasForeignKey(pd => pd.PackageId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasMany(p => p.Reservations)
                .WithOne(r => r.Package)
                .HasForeignKey(r => r.PackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasMany(p => p.Medias)
                .WithOne(m => m.Package)
                .HasForeignKey(m => m.PackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Hotel)
                .WithMany(h => h.Packages)
                .HasForeignKey(p => p.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Hotel
            modelBuilder.Entity<Hotel>()
               .HasOne(h => h.Address)
               .WithMany() // Remove the back reference - Address doesn't have Hotel property
               .HasForeignKey(h => h.AddressId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.RoomTypes)
                .WithOne(rt => rt.Hotel)
                .HasForeignKey(rt => rt.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.HotelDates)
                .WithOne(hd => hd.Hotel)
                .HasForeignKey(hd => hd.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Reservations)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Medias)
                .WithOne(m => m.Hotel)
                .HasForeignKey(m => m.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Reviews)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for HotelDate -> HotelRoomType
            modelBuilder.Entity<HotelDate>()
                .HasOne(hd => hd.HotelRoomType)
                .WithMany()
                .HasForeignKey(hd => hd.RoomTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.HotelRoomType)
                .WithMany()
                .HasForeignKey(r => r.RoomTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.BillingAddress)
                .WithMany()
                .HasForeignKey(p => p.BillingAddressId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuration for Companion
            modelBuilder.Entity<Companion>()
                .HasOne(c => c.Reservation)
                .WithMany(r => r.Companions)
                .HasForeignKey(c => c.ReservationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Hotel 1:N Commoditie
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Commodities) // Assuming Hotel has a collection of Commodities
                .WithOne(c => c.Hotel) // Assuming Commoditie has a Hotel property
                .HasForeignKey(c => c.HotelId) // Foreign key in Commoditie
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Hotel is deleted

            // Commodity 1:N CommoditiesServices
            modelBuilder.Entity<Commoditie>()
                .HasMany(c => c.CommoditieServices)
                .WithOne(cs => cs.Commoditie)
                .HasForeignKey(cs => cs.CommoditieId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommoditieServices 1:N Hotel
            modelBuilder.Entity<CommoditieServices>()
                .HasOne(cs => cs.Hotel)
                .WithMany(h => h.CommoditieServices)
                .HasForeignKey(cs => cs.HotelId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configuration for Media
            modelBuilder.Entity<Media>()
                .ToTable(t => t.HasCheckConstraint("CK_Media_OneEntity",
                    "([PackageId] IS NOT NULL AND [HotelId] IS NULL) OR ([PackageId] IS NULL AND [HotelId] IS NOT NULL)"));

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "CLIENT", IsActive = true },
                new Role { Id = 2, Name = "SERVICE_PROVIDER", IsActive = true },
                new Role { Id = 3, Name = "ATTENDANT", IsActive = true },
                new Role { Id = 4, Name = "ADMIN", IsActive = true }
            );

            // Global query filters for ISoftDeletable entities
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
            modelBuilder.Entity<Package>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<PackageDate>().HasQueryFilter(pd => pd.IsActive);
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => h.IsActive);
            modelBuilder.Entity<HotelRoomType>().HasQueryFilter(rt => rt.IsActive);
            modelBuilder.Entity<HotelDate>().HasQueryFilter(hd => hd.IsActive);
            modelBuilder.Entity<Reservation>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Address>().HasQueryFilter(a => a.IsActive);
            modelBuilder.Entity<Media>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<Review>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Companion>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Commoditie>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<CommoditieServices>().HasQueryFilter(cs => cs.IsActive);
            //modelBuilder.Entity<Role>().HasQueryFilter(r => r.IsActive);
        }
    }
}