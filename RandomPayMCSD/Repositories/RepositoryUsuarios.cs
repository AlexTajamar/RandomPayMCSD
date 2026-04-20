using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryUsuarios : ApiClientBase, IRepositoryUsuarios
    {
        public RepositoryUsuarios(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await GetAsync<List<Usuario>>("/apiRandomPay/Users") ?? new List<Usuario>();
        }

        public Task<Usuario?> GetByIdAsync(int id)
        {
            return GetAsync<Usuario>($"/apiRandomPay/Users/{id}");
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            var users = await GetAllAsync();
            return users.FirstOrDefault(x => x.EMAIL.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public Task AddAsync(Usuario usuario)
        {
            return PostAsync("/apiRandomPay/Users/Register", new
            {
                nombre = usuario.NOMBRE,
                email = usuario.EMAIL,
                password = usuario.PASSWORD
            });
        }

        public Task UpdateAsync(Usuario usuario)
        {
            return PutAsync("/apiRandomPay/Users", usuario);
        }

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"/apiRandomPay/Users/{id}");
        }
    }
}
