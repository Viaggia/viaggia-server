using Microsoft.EntityFrameworkCore;
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
using ViaggiaServer.Models.Packages;

namespace viaggia_server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets para todas as entidades
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageDate> PackageDates { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelRoomType> RoomTypes { get; set; }
        public DbSet<HotelDate> HotelDates { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de UserRole (chave composta)
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

            // Configuração de Package
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

            // Configuração de Hotel
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

            // Configuração de HotelDate -> HotelRoomType
            modelBuilder.Entity<HotelDate>()
                .HasOne(hd => hd.HotelRoomType)
                .WithMany() // No navigation property in HotelRoomType for HotelDates
                .HasForeignKey(hd => hd.RoomTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuração de Reservation
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

            // Configuração de Payment
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

            // Configuração de Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuração de Media
            modelBuilder.Entity<Media>()
                .ToTable(t => t.HasCheckConstraint("CK_Media_OneEntity",
                    "([PackageId] IS NOT NULL AND [HotelId] IS NULL) OR ([PackageId] IS NULL AND [HotelId] IS NOT NULL)"));

            // Filtros globais para entidades ISoftDeletable
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
            modelBuilder.Entity<Package>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<PackageDate>().HasQueryFilter(pd => pd.IsActive);
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => h.IsActive);
            modelBuilder.Entity<HotelRoomType>().HasQueryFilter(rt => rt.IsActive);
            modelBuilder.Entity<HotelDate>().HasQueryFilter(hd => hd.IsActive);
            modelBuilder.Entity<Reservation>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            modelBuilder.Entity<Media>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<Review>().HasQueryFilter(r => r.IsActive);
        }
    }
}