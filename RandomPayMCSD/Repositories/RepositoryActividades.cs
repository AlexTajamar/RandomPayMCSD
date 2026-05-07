using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryActividades : ApiClientBase, IRepositoryActividades
    {
        public RepositoryActividades(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<Actividad>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await GetAsync<List<Actividad>>("/apiRandomPay/Actividades/MisActividades") ?? new List<Actividad>();
        }

        public Task<Actividad?> GetByIdWithDetailsAsync(int id)
        {
            return GetAsync<Actividad>($"/apiRandomPay/Actividades/{id}");
        }

        public Task<Actividad?> GetByCodigoInvitacionAsync(string codigo)
        {
            return GetByCodigoAsync(codigo);
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            return await GetByCodigoAsync(codigo) != null;
        }

        public async Task<int> AddAsync(Actividad actividad)
        {
            Actividad? created = await PostAsync<Actividad, Actividad>("/apiRandomPay/Actividades", actividad);
            if (created?.IDACTIVIDAD > 0)
            {
                actividad.IDACTIVIDAD = created.IDACTIVIDAD;
                return created.IDACTIVIDAD;
            }

            if (actividad.IDACTIVIDAD > 0)
            {
                return actividad.IDACTIVIDAD;
            }

            if (!string.IsNullOrWhiteSpace(actividad.INVITACIONCOD))
            {
                Actividad? byCodigo = await GetByCodigoAsync(actividad.INVITACIONCOD);
                if (byCodigo?.IDACTIVIDAD > 0)
                {
                    actividad.IDACTIVIDAD = byCodigo.IDACTIVIDAD;
                    return byCodigo.IDACTIVIDAD;
                }
            }

            return 0;
        }

        public Task UpdateAsync(Actividad actividad)
        {
            return PutAsync("/apiRandomPay/Actividades", actividad);
        }

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"/apiRandomPay/Actividades/{id}");
        }

        public Task<Actividad?> GetByCodigoAsync(string codigo)
        {
            return GetAsync<Actividad>($"/apiRandomPay/Actividades/ByCodigo/{codigo.Trim().ToUpperInvariant()}");
        }

        public Task<Usuario?> GetUsuarioByIdAsync(int usuarioId)
        {
            return GetAsync<Usuario>($"/apiRandomPay/Actividades/Usuario/{usuarioId}");
        }
    }
}
