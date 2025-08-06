using System.Data.Common;
using API_Maquinas.Controllers;
using API_Maquinas.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;


namespace API_Maquinas.DTOs
{
    public class StoredContext : DbContext
    {
        public StoredContext(DbContextOptions<StoredContext> options) : base(options) { }

        public DbSet<Machines> Maquinas { get; set; }

        public DbSet<Logins> Logins { get; set; }

        public DbSet<Sales> Ventas { get; set; }
        public DbSet<SaleItem> VentaItems { get; set; }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<MaOrder> MaOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sales>()
                .HasOne(s => s.Cliente)
                .WithMany()
                .HasForeignKey(s => s.ClienteId)
                .IsRequired(false); 

            modelBuilder.Entity<Sales>()
                .Property(s => s.TotalVenta)
                .HasColumnType("decimal(18,2)");
        }

    }
}
