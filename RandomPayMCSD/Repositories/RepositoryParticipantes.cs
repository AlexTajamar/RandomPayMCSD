using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryParticipantes : IRepositoryParticipantes
    {
        private readonly RandomPayContext _context;

        public RepositoryParticipantes(RandomPayContext context)
        {
            _context = context;
        }

        public async Task<List<Participante>> GetByActividadIdAsync(int actividadId)
        {
            return await _context.Participantes
                .Where(p => p.IDACTIVIDAD == actividadId)
                .ToListAsync();
        }

        public async Task<Participante?> GetByIdAsync(int id)
        {
            return await _context.Participantes.FindAsync(id);
        }

        public async Task AddAsync(Participante participante)
        {
            await _context.Participantes.AddAsync(participante);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var participante = await GetByIdAsync(id);
            if (participante != null)
            {
                _context.Participantes.Remove(participante);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteParticipanteEnActividad(int actividadId, string nombre, int? idUsuario)
        {
            if (idUsuario.HasValue)
            {
                return await _context.Participantes
                    .AnyAsync(p => p.IDACTIVIDAD == actividadId && p.IDUSUARIO == idUsuario);
            }
            else
            {
                return await _context.Participantes
                    .AnyAsync(p => p.IDACTIVIDAD == actividadId && p.NOMBREPARTICIPANTE == nombre);
            }
        }
    }
}