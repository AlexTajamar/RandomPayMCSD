using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryActividades
    {
        Task<List<Actividad>> GetByUsuarioIdAsync(int usuarioId);
        Task<Actividad?> GetByIdWithDetailsAsync(int id); // Incluye participantes y gastos
        Task<Actividad?> GetByCodigoInvitacionAsync(string codigo);
        Task<bool> ExisteCodigoAsync(string codigo);
        Task AddAsync(Actividad actividad);
        Task UpdateAsync(Actividad actividad);
        Task DeleteAsync(int id);
        Task<Actividad> GetByCodigoAsync(string codigo);
        Task<Usuario> GetUsuarioByIdAsync(int usuarioId);
    }
}