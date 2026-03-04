using RandomPayMCSD.Models;

namespace RandomPayMCSD.Interfaces
{
    public interface IRepositoryDivisas
    {
        Task<List<Divisa>> GetDivisasAsync();
        Task<Divisa> GetDivisaByCodigoAsync(string codigo);
    }
}
