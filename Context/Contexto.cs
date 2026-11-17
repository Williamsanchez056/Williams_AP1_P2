using Williams_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;

namespace Williams_AP1_P2.Context;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options)
    {
    }

    public DbSet<Modelo> Modelos { get; set; }

    public DbSet<Productos> Productos { get; set; }

    public DbSet<Ventas> Ventas { get; set; }

    public DbSet<DetalleVenta> DetalleVentas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Productos>().HasData(
            new Productos { ProductoId = 1, Concepto = "Starbucks Frappuccino Vainilla", Precio = 150, Stock = 10 },
            new Productos { ProductoId = 2, Concepto = "Starbucks Frappuccino Mocha", Precio = 150, Stock = 10 },
            new Productos { ProductoId = 3, Concepto = "Cafe Latte Embotellado", Precio = 125, Stock = 10 },
            new Productos { ProductoId = 4, Concepto = "Cold Brew", Precio = 140, Stock = 10 }
        );

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId);
    }
}