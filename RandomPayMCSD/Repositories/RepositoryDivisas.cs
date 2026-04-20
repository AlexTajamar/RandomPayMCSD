using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryDivisas : ApiClientBase, IRepositoryDivisas
    {
        public RepositoryDivisas(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<Divisa>> GetDivisasAsync()
        {
            return await GetAsync<List<Divisa>>("/apiRandomPay/Statics/Divisas") ?? new List<Divisa>();
        }

        public async Task<Divisa?> GetDivisaByCodigoAsync(string codigo)
        {
            var divisas = await GetDivisasAsync();
            return divisas.FirstOrDefault(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        }
    }
}
