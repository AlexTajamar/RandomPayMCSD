using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryUsuarios : IRepositoryUsuarios
    {
        private readonly RandomPayContext _context;

        public RepositoryUsuarios(RandomPayContext context)
        {
                  this._context = context;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            var consulta = from datos in this._context.Usuarios
                           select datos;

            return await consulta.ToListAsync();
        }

        public async Task<Usuario> GetByIdAsync(int id)
        {
            var consulta = from datos in this._context.Usuarios
                           where datos.IDUSUARIO == id
                           select datos;

            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            var consulta = from datos in this._context.Usuarios
                           where datos.EMAIL == email
                           select datos;

            return await consulta.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Usuario usuario)
        {
            var consulta = from datos in this._context.Usuarios select datos.IDUSUARIO;

            if (await consulta.AnyAsync())
            {
                usuario.IDUSUARIO = await consulta.MaxAsync() + 1;
            }
            else
            {
                usuario.IDUSUARIO = 1;
            }

            await this._context.Usuarios.AddAsync(usuario);
            await this._context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            this._context.Usuarios.Update(usuario);
            await this._context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Usuario usuario = await this.GetByIdAsync(id);
            if (usuario != null)
            {
                this._context.Usuarios.Remove(usuario);
                await this._context.SaveChangesAsync();
            }
        }
    }
}