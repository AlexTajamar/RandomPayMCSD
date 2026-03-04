using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryRepartos
    {
        Task AddAsync(RepartoGasto reparto);
        Task<List<RepartoGasto>> GetRepartosByGastoAsync(int idGasto);
        Task DeleteAsync(int idReparto);
    }
}