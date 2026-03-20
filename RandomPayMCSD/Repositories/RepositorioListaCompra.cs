using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryListaCompra : IRepositoryListaCompra
    {
        private readonly RandomPayContext _context;

        public RepositoryListaCompra(RandomPayContext context)
        {
            _context = context;
        }

        public async Task<List<ItemCompra>> GetByActividadAsync(int idActividad)
        {
            return await _context.ItemsCompra
                .Where(x => x.IdActividad == idActividad)
                .OrderBy(x => x.Comprado).ThenByDescending(x => x.IdItem)
                .ToListAsync();
        }

        public async Task<ItemCompra> GetByIdAsync(int idItem)
        {
            return await _context.ItemsCompra.FindAsync(idItem);
        }

        public async Task AddAsync(ItemCompra item)
        {
            var consulta = from datos in this._context.ItemsCompra select datos.IdItem;
            item.IdItem = await consulta.AnyAsync() ? await consulta.MaxAsync() + 1 : 1;
            item.Comprado = false;
            await _context.ItemsCompra.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemCompra item)
        {
            _context.ItemsCompra.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int idItem)
        {
            var item = await GetByIdAsync(idItem);
            if (item != null)
            {
                _context.ItemsCompra.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}