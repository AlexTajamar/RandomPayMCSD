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
            var consulta = from datos in this._context.Participantes
                           where datos.IDACTIVIDAD == actividadId
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<Participante?> GetByIdAsync(int id)
        {
            var consulta = from datos in this._context.Participantes
                           where datos.IDPARTICIPANTE == id
                           select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        // --- AÑADE ESTE MÉTODO QUE FALTA ---
        public async Task<bool> ExisteParticipanteEnActividad(int idActividad, string nombre, int? idUsuario)
        {
            var consulta = from datos in this._context.Participantes
                           where datos.IDACTIVIDAD == idActividad
                              && datos.NOMBREPARTICIPANTE == nombre
                           select datos;
            return await consulta.AnyAsync();
        }

        public async Task AddAsync(Participante participante)
        {
            var consulta = from datos in this._context.Participantes select datos.IDPARTICIPANTE;
            participante.IDPARTICIPANTE = await consulta.AnyAsync() ? await consulta.MaxAsync() + 1 : 1;
            await this._context.Participantes.AddAsync(participante);
            await this._context.SaveChangesAsync();
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
    }
}