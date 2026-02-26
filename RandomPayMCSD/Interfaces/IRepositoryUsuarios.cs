using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryUsuarios
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario> GetByIdAsync(int id);
        Task<Usuario> GetByEmailAsync(string email);
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(int id);
    }
}