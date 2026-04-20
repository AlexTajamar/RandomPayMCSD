using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryListaCompra : ApiClientBase, IRepositoryListaCompra
    {
        public RepositoryListaCompra(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<ItemCompra>> GetByActividadAsync(int idActividad)
        {
            return await GetAsync<List<ItemCompra>>($"/apiRandomPay/ListaCompra/PorActividad/{idActividad}") ?? new List<ItemCompra>();
        }

        public Task<ItemCompra?> GetByIdAsync(int idItem)
        {
            return GetAsync<ItemCompra>($"/apiRandomPay/ListaCompra/{idItem}");
        }

        public Task AddAsync(ItemCompra item)
        {
            return PostAsync("/apiRandomPay/ListaCompra", item);
        }

        public Task UpdateAsync(ItemCompra item)
        {
            return PutAsync("/apiRandomPay/ListaCompra", item);
        }

        public Task DeleteAsync(int idItem)
        {
            return DeleteAsync($"/apiRandomPay/ListaCompra/{idItem}");
        }
    }
}
