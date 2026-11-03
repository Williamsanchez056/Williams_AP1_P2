using Williams_AP1_P2.Context;
using Williams_AP1_P2.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Williams_AP1_P2.Services;

public class ModeloService
{
    private readonly IDbContextFactory<Contexto> DbFactory;

    public ModeloService(IDbContextFactory<Contexto> dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task<List<Modelo>> GetList(Expression<Func<Modelo, bool>> criterio)
    {
        using var contexto = DbFactory.CreateDbContext();
        return await contexto.Modelos.Where(criterio).ToListAsync();
    }

    public async Task<Modelo?> Buscar(int id)
    {
        using var contexto = DbFactory.CreateDbContext();
        return await contexto.Modelos.FirstOrDefaultAsync(m => m.ModeloId == id);
    }

    public async Task<bool> Guardar(Modelo modelo)
    {
        using var contexto = DbFactory.CreateDbContext();

        if (modelo.ModeloId == 0)
            contexto.Modelos.Add(modelo);
        else
        {
            var modeloExiste = await contexto.Modelos.FindAsync(modelo.ModeloId);
            if (modeloExiste == null)
                return false;

            modeloExiste.Nombre = modelo.Nombre;
        }

        await contexto.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Eliminar(int id)
    {
        using var contexto = DbFactory.CreateDbContext();
        var modelo = await contexto.Modelos.FindAsync(id);

        if (modelo == null)
            return false;

        contexto.Modelos.Remove(modelo);
        await contexto.SaveChangesAsync();
        return true;
    }
}