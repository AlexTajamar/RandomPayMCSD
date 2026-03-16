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
            this._context = context;
        }

        public async Task<List<Actividad>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Actividades
                .Include(a => a.Gastos) // <-- AÑADIDO: Extrae los gastos para poder sumarlos
                .Include(a => a.Participantes) // <-- AÑADIDO: Útil para ver cuánta gente hay
                .Where(a => a.Participantes.Any(p => p.IDUSUARIO == usuarioId))
                .ToListAsync();
        }

        public async Task<Actividad> GetByCodigoAsync(string codigo)
        {
            string codigoLimpio = codigo.Trim().ToUpper();
            return await _context.Actividades
                .Include(a => a.Participantes)
                .FirstOrDefaultAsync(x => x.INVITACIONCOD == codigoLimpio);
        }

        public async Task<Actividad?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Actividades
                .Include(a => a.Participantes)
                .Include(a => a.Gastos)
                    .ThenInclude(g => g.Pagador)
                .Include(a => a.Gastos)
                    .ThenInclude(g => g.Repartos) // Ahora esto ya no dará error
                .FirstOrDefaultAsync(a => a.IDACTIVIDAD == id);
        }

        public async Task<Actividad?> GetByCodigoInvitacionAsync(string codigo)
        {
            return await _context.Actividades
                .FirstOrDefaultAsync(x => x.INVITACIONCOD == codigo);
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            return await _context.Actividades
                .AnyAsync(x => x.INVITACIONCOD == codigo);
        }

        public async Task AddAsync(Actividad actividad)
        {
            var maxId = await _context.Actividades.AnyAsync()
                ? await _context.Actividades.MaxAsync(a => a.IDACTIVIDAD)
                : 0;
            actividad.IDACTIVIDAD = maxId + 1;

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