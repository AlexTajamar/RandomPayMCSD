using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryActividades : IRepositoryActividades
    {
        private readonly RandomPayContext _context;

        public RepositoryActividades(RandomPayContext context)
        {
            _context = context;
        }

        public async Task<List<Actividad>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Actividades
                .Where(a => a.Participantes.Any(p => p.IDUSUARIO == usuarioId))
                .ToListAsync();
        }

        public async Task<Actividad?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Actividades
                .Include(a => a.Participantes)
                    .ThenInclude(p => p.Usuario)
                .Include(a => a.Gastos)
                    .ThenInclude(g => g.Pagador)
                .FirstOrDefaultAsync(a => a.IDACTIVIDAD == id);
        }

        public async Task<Actividad?> GetByCodigoInvitacionAsync(string codigo)
        {
            return await _context.Actividades
                .Include(a => a.Participantes)
                .FirstOrDefaultAsync(a => a.INVITACIONCOD == codigo);
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            return await _context.Actividades.AnyAsync(a => a.INVITACIONCOD == codigo);
        }

        public async Task AddAsync(Actividad actividad)
        {
            await _context.Actividades.AddAsync(actividad);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Actividad actividad)
        {
            _context.Actividades.Update(actividad);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var actividad = await _context.Actividades.FindAsync(id);
            if (actividad != null)
            {
                _context.Actividades.Remove(actividad);
                await _context.SaveChangesAsync();
            }
        }
    }
}