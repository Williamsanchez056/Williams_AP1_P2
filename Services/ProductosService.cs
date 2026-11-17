using Aplicada1.Core;
using Williams_AP1_P2.Context;
using Williams_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Williams_AP1_P2.Services;

public class ProductosService : IServiceResult<Productos, int>
{
    private readonly IDbContextFactory<Contexto> DbFactory;

    public ProductosService(IDbContextFactory<Contexto> dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task<Result<List<Productos>>> GetList(Expression<Func<Productos, bool>> criterio)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            var productos = await contexto.Productos
                .Where(criterio)
                .ToListAsync();

            return Result.Success(productos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Productos>>(
                new Error("PRODUCTOS_LISTA", ex.Message));
        }
    }

    public async Task<Result<Productos?>> Buscar(int id)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            var producto = await contexto.Productos
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
            {
                return Result.Failure<Productos?>(
                    new Error("PRODUCTO_NO_ENCONTRADO", "El producto no existe."));
            }

            return Result.Success<Productos?>(producto);
        }
        catch (Exception ex)
        {
            return Result.Failure<Productos?>(
                new Error("PRODUCTO_BUSCAR", ex.Message));
        }
    }

    public async Task<Result> Guardar(Productos entidad)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            if (entidad.ProductoId == 0)
            {
                contexto.Productos.Add(entidad);
            }
            else
            {
                contexto.Productos.Update(entidad);
            }

            await contexto.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("PRODUCTO_GUARDAR", ex.Message));
        }
    }

    public async Task<Result> Eliminar(int id)
    {
        try
        {
            using var contexto = DbFactory.CreateDbContext();

            var producto = await contexto.Productos
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
            {
                return Result.Failure(
                    new Error("PRODUCTO_NO_ENCONTRADO", "El producto no existe."));
            }

            contexto.Productos.Remove(producto);

            await contexto.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                new Error("PRODUCTO_ELIMINAR", ex.Message));
        }
    }
}
