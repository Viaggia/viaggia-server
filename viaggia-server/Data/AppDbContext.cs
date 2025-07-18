using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;
using viaggia_server.Models.Destination;
using viaggia_server.Models.Package;
using viaggia_server.Models.User;

namespace viaggia_server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Usuarios { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UsuarioRoles { get; set; }
        public DbSet<Destination> Destinations { get; set; }

        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageDate> PackageDates { get; set; }
        //public DbSet<PacoteMidia> PacoteMidias { get; set; }
        //public DbSet 
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<PackageDateRoomType> PackageDateRoomTypes { get; set; }
        //public DbSet<Reserva> Reservas { get; set; }
        //public DbSet<Acompanhante> Acompanhantes { get; set; }
        //public DbSet<Pagamento> Pagamentos { get; set; }  
        //public DbSet<HistoricoCompra> HistoricoCompras { get; set; }
        //public DbSet<Avaliacao> Avaliacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chave composta para UsuarioRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Destination)
                .WithMany(d => d.Packages)
                .HasForeignKey(p => p.DestinationId);

            modelBuilder.Entity<PackageDate>()
                .HasOne(pd => pd.Package)
                .WithMany(p => p.PackageDates)
                .HasForeignKey(pd => pd.PackageId);

            modelBuilder.Entity<PackageDateRoomType>()
                .HasOne(pq => pq.PackageDate)
                .WithMany(pd => pd.PackageDateRoomTypes)
                .HasForeignKey(pq => pq.PackageDateId);

            modelBuilder.Entity<PackageDateRoomType>()
                .HasOne(pq => pq.RoomType)
                .WithMany(tq => tq.PackageDateRooms)
                .HasForeignKey(pq => pq.RoomTypeId);

            //modelBuilder.Entity<Reserva>()
            //    .HasOne(r => r.Usuario)
            //    .WithMany(u => u.Reservas)
            //    .HasForeignKey(r => r.UsuarioId);

            //modelBuilder.Entity<Reserva>()
            //    .HasOne(r => r.PacoteDataQuarto)
            //    .WithMany(pq => pq.Reservas)
            //    .HasForeignKey(r => r.PacoteDataQuartoId);

            //modelBuilder.Entity<Acompanhante>()
            //    .HasOne(a => a.Reserva)
            //    .WithMany(r => r.Acompanhantes)
            //    .HasForeignKey(a => a.ReservaId);

            //modelBuilder.Entity<Pagamento>()
            //    .HasOne(p => p.Reserva)
            //    .WithMany(r => r.Pagamentos)
            //    .HasForeignKey(p => p.ReservaId);

            //modelBuilder.Entity<HistoricoCompra>()
            //    .HasOne(h => h.Usuario)
            //    .WithMany(u => u.HistoricoCompras)
            //    .HasForeignKey(h => h.UsuarioId);

            //modelBuilder.Entity<HistoricoCompra>()
            //    .HasOne(h => h.Reserva)
            //    .WithMany(r => r.HistoricoCompras)
            //    .HasForeignKey(h => h.ReservaId);

            //modelBuilder.Entity<Avaliacao>()
            //    .HasOne(a => a.Usuario)
            //    .WithMany(u => u.Avaliacoes)
            //    .HasForeignKey(a => a.UsuarioId);

            //modelBuilder.Entity<Avaliacao>()
            //    .HasOne(a => a.Pacote)
            //    .WithMany(p => p.Avaliacoes)
            //    .HasForeignKey(a => a.PacoteId);
        }
    }
}