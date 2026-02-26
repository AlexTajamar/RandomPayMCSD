using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryGastos
    {
        Task<List<Gasto>> GetByActividadIdAsync(int actividadId);
        Task<Gasto?> GetByIdAsync(int id);
        Task AddAsync(Gasto gasto);
        Task UpdateAsync(Gasto gasto);
        Task DeleteAsync(int id);
    }
}