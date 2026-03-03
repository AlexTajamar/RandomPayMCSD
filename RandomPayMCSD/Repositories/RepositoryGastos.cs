using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryGastos : IRepositoryGastos
    {
        private readonly RandomPayContext _context;

        public RepositoryGastos(RandomPayContext context)
        {
            this._context = context;
        }

        public async Task<List<Gasto>> GetByActividadIdAsync(int actividadId)
        {
            var consulta = from datos in this._context.Gastos
                           where datos.IDACTIVIDAD == actividadId
                           select datos;

            return await consulta.ToListAsync();
        }

        public async Task<Gasto?> GetByIdAsync(int id)
        {
            var consulta = from datos in this._context.Gastos
                           where datos.IDGASTO == id
                           select datos;

            return await consulta.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Gasto gasto)
        {
            var consulta = from datos in this._context.Gastos select datos.IDGASTO;

            if (await consulta.AnyAsync())
            {
                gasto.IDGASTO = await consulta.MaxAsync() + 1;
            }
            else
            {
                gasto.IDGASTO = 1;
            }

            await this._context.Gastos.AddAsync(gasto);
            await this._context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Gasto gasto)
        {
            this._context.Gastos.Update(gasto);
            await this._context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Gasto gasto = await this.GetByIdAsync(id);
            if (gasto != null)
            {
                this._context.Gastos.Remove(gasto);
                await this._context.SaveChangesAsync();
            }
        }
    }
}