using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.HotelRoomTypes;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Payments;
using viaggia_server.Models.Reservations;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.RevokedToken;
using viaggia_server.Models.UserRoles;
using viaggia_server.Models.Users;
using viaggia_server.Models.RoomTypeEnums;

namespace viaggia_server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<PackageDate> PackageDates { get; set; } = null!;
        public DbSet<Hotel> Hotels { get; set; } = null!;
        public DbSet<HotelRoomType> RoomTypes { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Media> Medias { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Companion> Companions { get; set; } = null!;
        public DbSet<Commoditie> Commodities { get; set; } = null!;
        public DbSet<CommoditieServices> CommoditieServices { get; set; } = null!;
        public DbSet<RevokedToken> RevokedTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserRole
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

            // Package
            modelBuilder.Entity<Package>()
                .HasMany(p => p.PackageDates)
                .WithOne(pd => pd.Package)
                .HasForeignKey(pd => pd.PackageId)
                .OnDelete(DeleteBehavior.Cascade); // Enable cascade delete

            modelBuilder.Entity<Package>()
                .HasMany(p => p.Medias)
                .WithOne(m => m.Package)
                .HasForeignKey(m => m.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Package>()
                .HasMany(p => p.Reservations)
                .WithOne(r => r.Package)
                .HasForeignKey(r => r.PackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Hotel)
                .WithMany(h => h.Packages)
                .HasForeignKey(p => p.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            // Hotel
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.RoomTypes)
                .WithOne(rt => rt.Hotel)
                .HasForeignKey(rt => rt.HotelId)
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

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Commodities)
                .WithOne(c => c.Hotel)
                .HasForeignKey(c => c.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure RoomTypeEnum
            modelBuilder.Entity<HotelRoomType>()
                .Property(rt => rt.Name)
                .HasConversion(
                    v => v.ToString(),
                    v => (RoomTypeEnum)Enum.Parse(typeof(RoomTypeEnum), v));

            // Commoditie
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Commodities)
                .WithOne(c => c.Hotel)
                .HasForeignKey(c => c.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Commodity 1:N CommoditiesServices
            modelBuilder.Entity<Commoditie>()
              .HasMany(c => c.CommoditieServices)
              .WithOne(cs => cs.Commoditie)
              .HasForeignKey(cs => cs.CommoditieId)
              .OnDelete(DeleteBehavior.NoAction); 

            // CommoditieServices 1:N Hotel
            modelBuilder.Entity<CommoditieServices>()
                .HasOne(cs => cs.Hotel)
                .WithMany(h => h.CommoditieServices)
                .HasForeignKey(cs => cs.HotelId)
                .OnDelete(DeleteBehavior.NoAction);


            // Reservation
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

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // PasswordResetToken
            // Companion
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
              .OnDelete(DeleteBehavior.NoAction); // Evita ciclos de deleção 

            // CommoditieServices 1:N Hotel
            modelBuilder.Entity<CommoditieServices>()
                .HasOne(cs => cs.Hotel)
                .WithMany(h => h.CommoditieServices)
                .HasForeignKey(cs => cs.HotelId)
                .OnDelete(DeleteBehavior.NoAction); // Evita ciclos de deleção

            //  PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany()
                .HasForeignKey(prt => prt.UserId);

            // RevokedToken
            modelBuilder.Entity<RevokedToken>(entity =>
            {
                entity.ToTable("RevokedTokens");
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Id).ValueGeneratedOnAdd();
                entity.Property(rt => rt.Token).HasColumnType("nvarchar(max)").IsRequired();
                entity.Property(rt => rt.RevokedAt).IsRequired();
                entity.Property(rt => rt.ExpiryDate).IsRequired(false); 
            });


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

            // Global query filters
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
            modelBuilder.Entity<Package>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<PackageDate>().HasQueryFilter(pd => pd.IsActive);
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => h.IsActive);
            modelBuilder.Entity<HotelRoomType>().HasQueryFilter(rt => rt.IsActive);
            modelBuilder.Entity<Reservation>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Media>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<Review>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Commoditie>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<CommoditieServices>().HasQueryFilter(cs => cs.IsActive);
        }
    }
}