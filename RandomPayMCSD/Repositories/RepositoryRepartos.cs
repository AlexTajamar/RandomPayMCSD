using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryRepartos : ApiClientBase, IRepositoryRepartos
    {
        public RepositoryRepartos(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public Task AddAsync(RepartoGasto reparto)
        {
            return PostAsync($"/apiRandomPay/Gastos/{reparto.IdGasto}/Repartos", reparto);
        }

        public async Task<List<RepartoGasto>> GetRepartosByGastoAsync(int idGasto)
        {
            return await GetAsync<List<RepartoGasto>>($"/apiRandomPay/Gastos/{idGasto}/Repartos") ?? new List<RepartoGasto>();
        }

        public Task DeleteAsync(int idReparto)
        {
            return DeleteAsync($"/apiRandomPay/Gastos/Repartos/{idReparto}");
        }
    }
}
