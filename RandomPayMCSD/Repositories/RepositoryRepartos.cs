using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryRepartos : IRepositoryRepartos
    {
        private RandomPayContext context;

        public RepositoryRepartos(RandomPayContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(RepartoGasto reparto)
        {
            var consulta = from datos in this.context.RepartosGasto select datos.IdReparto;
            int maxId = await consulta.AnyAsync() ? await consulta.MaxAsync() : 0;
            reparto.IdReparto = maxId + 1;

            await this.context.RepartosGasto.AddAsync(reparto);
            await this.context.SaveChangesAsync();
        }

        public async Task<List<RepartoGasto>> GetRepartosByGastoAsync(int idGasto)
        {
            return await this.context.RepartosGasto
                .Where(r => r.IdGasto == idGasto)
                .ToListAsync();
        }

        public async Task DeleteAsync(int idReparto)
        {
            RepartoGasto reparto = await this.context.RepartosGasto.FindAsync(idReparto);
            if (reparto != null)
            {
                this.context.RepartosGasto.Remove(reparto);
                await this.context.SaveChangesAsync();
            }
        }
    }
}