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
            var consulta = from datos in this._context.Actividades
                           where datos.Participantes.Any(p => p.IDUSUARIO == usuarioId)
                           select datos;

            return await consulta.ToListAsync();
        }

        public async Task<Actividad?> GetByIdWithDetailsAsync(int id)
        {
            // Para traer tablas relacionadas usamos .Include() dentro del contexto antes del from
            var consulta = from datos in this._context.Actividades
                                  .Include(a => a.Participantes)
                                  .Include(a => a.Gastos)
                                  .ThenInclude(g => g.Pagador)
                           where datos.IDACTIVIDAD == id
                           select datos;

            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<Actividad?> GetByCodigoInvitacionAsync(string codigo)
        {
            var consulta = from datos in this._context.Actividades
                           where datos.INVITACIONCOD == codigo
                           select datos;

            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            var consulta = from datos in this._context.Actividades
                           where datos.INVITACIONCOD == codigo
                           select datos;

            return await consulta.AnyAsync();
        }

        public async Task AddAsync(Actividad actividad)
        {
            var consulta = from datos in this._context.Actividades select datos.IDACTIVIDAD;

            if (await consulta.AnyAsync())
            {
                actividad.IDACTIVIDAD = await consulta.MaxAsync() + 1;
            }
            else
            {
                actividad.IDACTIVIDAD = 1;
            }

            await this._context.Actividades.AddAsync(actividad);
            await this._context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Actividad actividad)
        {
            this._context.Actividades.Update(actividad);
            await this._context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var consulta = from datos in this._context.Actividades
                           where datos.IDACTIVIDAD == id
                           select datos;

            Actividad actividad = await consulta.FirstOrDefaultAsync();

            if (actividad != null)
            {
                this._context.Actividades.Remove(actividad);
                await this._context.SaveChangesAsync();
            }

        }
        
    }
}