using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryGastos : ApiClientBase, IRepositoryGastos
    {
        public RepositoryGastos(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<Gasto>> GetByActividadIdAsync(int actividadId)
        {
            return await GetAsync<List<Gasto>>($"/apiRandomPay/Gastos/PorActividad/{actividadId}") ?? new List<Gasto>();
        }

        public Task<Gasto?> GetByIdAsync(int id)
        {
            return GetAsync<Gasto>($"/apiRandomPay/Gastos/{id}");
        }

        public async Task<int> AddAsync(Gasto gasto)
        {
            Gasto? created = await PostAsync<Gasto, Gasto>("/apiRandomPay/Gastos", gasto);
            if (created?.IDGASTO > 0)
            {
                gasto.IDGASTO = created.IDGASTO;
                return created.IDGASTO;
            }

            if (gasto.IDGASTO > 0)
            {
                return gasto.IDGASTO;
            }

            List<Gasto> gastos = await GetByActividadIdAsync(gasto.IDACTIVIDAD);
            Gasto? match = gastos
                .Where(g => g.IDPAGADOR == gasto.IDPAGADOR
                    && g.IMPORTE == gasto.IMPORTE
                    && string.Equals(g.CONCEPTO?.Trim(), gasto.CONCEPTO?.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(g => g.FECHA)
                .ThenByDescending(g => g.IDGASTO)
                .FirstOrDefault();

            if (match?.IDGASTO > 0)
            {
                gasto.IDGASTO = match.IDGASTO;
                return match.IDGASTO;
            }

            return 0;
        }

        public async Task UpdateAsync(Gasto gasto)
        {
            await PutAsync("/apiRandomPay/Gastos", gasto);
        }

        public async Task DeleteAsync(int id)
        {
            await DeleteAsync($"/apiRandomPay/Gastos/{id}");
        }
    }
}
