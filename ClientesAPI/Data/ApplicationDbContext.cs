using Microsoft.EntityFrameworkCore;
using ClientesAPI.Models;

namespace ClientesAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets representan las tablas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ArchivoCliente> ArchivosCliente { get; set; }
        public DbSet<LogApi> LogsApi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar Cliente primero
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.CI);
                entity.Property(c => c.CI).HasMaxLength(20);
                entity.Property(c => c.Nombres).HasMaxLength(200).IsRequired();
                entity.Property(c => c.Direccion).HasMaxLength(300).IsRequired();
                entity.Property(c => c.Telefono).HasMaxLength(20).IsRequired();
            });

            // Configurar ArchivoCliente con su relación
            modelBuilder.Entity<ArchivoCliente>(entity =>
            {
                entity.HasKey(a => a.IdArchivo);

                entity.HasOne(a => a.Cliente)
                    .WithMany(c => c.Archivos)
                    .HasForeignKey(a => a.CICliente)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.CICliente);
            });

            // Configurar LogApi
            modelBuilder.Entity<LogApi>(entity =>
            {
                entity.HasKey(l => l.IdLog);
            });
        }
    }
}