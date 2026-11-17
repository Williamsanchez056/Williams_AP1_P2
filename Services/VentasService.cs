using Aplicada1.Core;
using Williams_AP1_P2.Context;
using Williams_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Williams_AP1_P2.Services;

public class VentasService : IServiceResult<Ventas, int>
{
    private readonly IDbContextFactory<Contexto> DbFactory;

    public VentasService(IDbContextFactory<Contexto> dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task<Result<List<Ventas>>> GetList(Expression<Func<Ventas, bool>> criterio)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            var ventas = await contexto.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(criterio)
                .ToListAsync();

            return Result.Success(ventas);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Ventas>>(
                new Error("VENTAS_LISTA", ex.Message));
        }
    }

    public async Task<Result<Ventas?>> Buscar(int id)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            var venta = await contexto.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
            {
                return Result.Failure<Ventas?>(
                    new Error("VENTA_NO_ENCONTRADA", "La venta no existe."));
            }

            return Result.Success<Ventas?>(venta);
        }
        catch (Exception ex)
        {
            return Result.Failure<Ventas?>(
                new Error("VENTA_BUSCAR", ex.Message));
        }
    }

    public async Task<Result> Guardar(Ventas venta)
    {
        using var contexto = DbFactory.CreateDbContext();
        using var transaccion = await contexto.Database.BeginTransactionAsync();

        try
        {
            if (venta.Detalles == null || venta.Detalles.Count == 0)
            {
                await transaccion.RollbackAsync();

                return Result.Failure(
                    new Error("VENTA_SIN_DETALLES", "Debe agregar productos a la venta."));
            }

            var ventaExiste = venta.VentaId == 0
                ? null
                : await contexto.Ventas
                    .Include(v => v.Detalles)
                    .FirstOrDefaultAsync(v => v.VentaId == venta.VentaId);

            if (venta.VentaId != 0 && ventaExiste == null)
            {
                await transaccion.RollbackAsync();

                return Result.Failure(
                    new Error("VENTA_NO_ENCONTRADA", "La venta no existe."));
            }

            if (ventaExiste != null)
            {
                foreach (var detalleAnterior in ventaExiste.Detalles)
                {
                    var producto = await contexto.Productos
                        .FirstOrDefaultAsync(p => p.ProductoId == detalleAnterior.ProductoId);

                    if (producto == null)
                    {
                        await transaccion.RollbackAsync();

                        return Result.Failure(
                            new Error("PRODUCTO_NO_ENCONTRADO", "No se encontró un producto de la venta."));
                    }

                    producto.Stock += detalleAnterior.Cantidad;
                }

                contexto.DetalleVentas.RemoveRange(ventaExiste.Detalles);
                ventaExiste.Detalles.Clear();
            }

            decimal total = 0;

            foreach (var detalle in venta.Detalles)
            {
                if (detalle.Cantidad <= 0)
                {
                    await transaccion.RollbackAsync();

                    return Result.Failure(
                        new Error("CANTIDAD_INVALIDA", "La cantidad debe ser mayor que cero."));
                }

                var producto = await contexto.Productos
                    .FirstOrDefaultAsync(p => p.ProductoId == detalle.ProductoId);

                if (producto == null)
                {
                    await transaccion.RollbackAsync();

                    return Result.Failure(
                        new Error("PRODUCTO_NO_ENCONTRADO", "El producto seleccionado no existe."));
                }

                if (producto.Stock < detalle.Cantidad)
                {
                    await transaccion.RollbackAsync();

                    return Result.Failure(
                        new Error("STOCK_INSUFICIENTE", $"Stock insuficiente para {producto.Concepto}."));
                }

                detalle.Precio = producto.Precio;

                producto.Stock -= detalle.Cantidad;

                total += detalle.Cantidad * detalle.Precio;
            }

            if (ventaExiste == null)
            {
                ventaExiste = new Ventas
                {
                    Fecha = venta.Fecha,
                    Observacion = venta.Observacion,
                    Total = total
                };

                contexto.Ventas.Add(ventaExiste);
            }
            else
            {
                ventaExiste.Fecha = venta.Fecha;
                ventaExiste.Observacion = venta.Observacion;
                ventaExiste.Total = total;
            }

            await contexto.SaveChangesAsync();

            foreach (var detalle in venta.Detalles)
            {
                detalle.DetalleId = 0;
                detalle.VentaId = ventaExiste.VentaId;
                detalle.Producto = null;
                detalle.Venta = null;

                contexto.DetalleVentas.Add(detalle);
            }

            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();

            return Result.Failure(
                new Error("VENTA_GUARDAR", ex.Message));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        using var contexto = DbFactory.CreateDbContext();
        using var transaccion = await contexto.Database.BeginTransactionAsync();

        try
        {
            var venta = await contexto.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
            {
                await transaccion.RollbackAsync();

                return Result.Failure(
                    new Error("VENTA_NO_ENCONTRADA", "La venta no existe."));
            }

            foreach (var detalle in venta.Detalles)
            {
                var producto = await contexto.Productos
                    .FirstOrDefaultAsync(p => p.ProductoId == detalle.ProductoId);

                if (producto == null)
                {
                    await transaccion.RollbackAsync();

                    return Result.Failure(
                        new Error("PRODUCTO_NO_ENCONTRADO", "No se encontró un producto de la venta."));
                }

                producto.Stock += detalle.Cantidad;
            }

            contexto.Ventas.Remove(venta);

            await contexto.SaveChangesAsync();
            await transaccion.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();

            return Result.Failure(
                new Error("VENTA_ELIMINAR", ex.Message));
        }
    }
}
