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
            _context = context;
        }

        public async Task<List<Gasto>> GetByActividadIdAsync(int actividadId)
        {
            return await _context.Gastos
                .Where(g => g.IDACTIVIDAD == actividadId)
                .Include(g => g.Pagador)
                .ToListAsync();
        }

        public async Task<Gasto?> GetByIdAsync(int id)
        {
            return await _context.Gastos.FindAsync(id);
        }

        public async Task AddAsync(Gasto gasto)
        {
            await _context.Gastos.AddAsync(gasto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Gasto gasto)
        {
            _context.Gastos.Update(gasto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var gasto = await GetByIdAsync(id);
            if (gasto != null)
            {
                _context.Gastos.Remove(gasto);
                await _context.SaveChangesAsync();
            }
        }
    }
}