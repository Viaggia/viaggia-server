using Microsoft.EntityFrameworkCore;
using viaggia_server.Models.Commodities;
using viaggia_server.Models.CustomCommodities;
using viaggia_server.Models.Hotels;
using viaggia_server.Models.Medias;
using viaggia_server.Models.Packages;
using viaggia_server.Models.Reserves;
using viaggia_server.Models.Reviews;
using viaggia_server.Models.Users;

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
        public DbSet<Commodity> Commodities { get; set; } = null!;
        public DbSet<CustomCommodity> CustomCommodities { get; set; } = null!;
        public DbSet<Reserve> Reserves { get; set; } = null!;
        public DbSet<Media> Medias { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<RevokedToken> RevokedTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Hotel)
                .WithOne(h => h.User)
                .HasForeignKey<User>("HotelId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

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
                .HasMany(p => p.Reserves)
                .WithOne(r => r.Package)
                .HasForeignKey(r => r.PackageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Hotel)
                .WithMany(h => h.Packages)
                .HasForeignKey(p => p.HotelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.User)
                .WithMany(u => u.Packages) // Assuming you add a Packages collection to User
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Hotel
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.RoomTypes)
                .WithOne(rt => rt.Hotel)
                .HasForeignKey(rt => rt.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Reserves)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Medias)
                .WithOne(m => m.Hotel)
                .HasForeignKey(m => m.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Reviews)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Commodities)
                .WithOne(c => c.Hotel)
                .HasForeignKey(c => c.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.CustomCommodities)
                .WithOne(cs => cs.Hotel)
                .HasForeignKey(cs => cs.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HotelRoomType>()
                .Property(rt => rt.Name)
                .HasConversion(
                    v => v.ToString(),
                    v => (RoomTypeEnum)Enum.Parse(typeof(RoomTypeEnum), v));


            modelBuilder.Entity<Commodity>()
              .HasMany(c => c.CustomCommodities)
              .WithOne(cs => cs.Commodity)
              .HasForeignKey(cs => cs.CommodityId)
              .OnDelete(DeleteBehavior.NoAction);

            // CommoditieServices 1:N Hotel
            modelBuilder.Entity<CustomCommodity>()
                .HasOne(cs => cs.Hotel)
                .WithMany(h => h.CustomCommodities)
                .HasForeignKey(cs => cs.HotelId);
            
            // Commodity
            modelBuilder.Entity<Commodity>()
                .HasMany(c => c.CustomCommodities)
                .WithOne(cs => cs.Commodity)
                .HasForeignKey(cs => cs.CommodityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReserveRoom>()
                .HasOne(rr => rr.Reserve)
                .WithMany(r => r.ReserveRooms)
                .HasForeignKey(rr => rr.ReserveId);

            modelBuilder.Entity<ReserveRoom>()
                .HasOne(rr => rr.RoomType)
                .WithMany()
                .HasForeignKey(rr => rr.RoomTypeId);


            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            // Hotel 1:N Commoditie
            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Commodities) // Assuming Hotel has a collection of Commodities
                .WithOne(c => c.Hotel) // Assuming Commoditie has a Hotel property
                .HasForeignKey(c => c.HotelId) // Foreign key in Commoditie
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Hotel is deleted

            // Commodity 1:N CommoditiesServices
            modelBuilder.Entity<Commodity>()
              .HasMany(c => c.CustomCommodities)
              .WithOne(cs => cs.Commodity)
              .HasForeignKey(cs => cs.CommodityId)
              .OnDelete(DeleteBehavior.NoAction); // Evita ciclos de deleção 

            // CommoditieServices 1:N Hotel
            modelBuilder.Entity<CustomCommodity>()
                .HasOne(cs => cs.Hotel)
                .WithMany(h => h.CustomCommodities)
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
            modelBuilder.Entity<PackageDate>().HasQueryFilter(pd => pd.IsActive);
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => h.IsActive);
            modelBuilder.Entity<HotelRoomType>().HasQueryFilter(rt => rt.IsActive);
            modelBuilder.Entity<Reserve>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Media>().HasQueryFilter(m => m.IsActive);
            modelBuilder.Entity<Review>().HasQueryFilter(r => r.IsActive);
            modelBuilder.Entity<Commodity>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<CustomCommodity>().HasQueryFilter(cs => cs.IsActive);
        }
    }
}