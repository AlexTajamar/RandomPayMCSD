using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryListaCompra
    {
        Task<List<ItemCompra>> GetByActividadAsync(int idActividad);
        Task<ItemCompra> GetByIdAsync(int idItem);
        Task AddAsync(ItemCompra item);
        Task UpdateAsync(ItemCompra item);
        Task DeleteAsync(int idItem);
    }
}