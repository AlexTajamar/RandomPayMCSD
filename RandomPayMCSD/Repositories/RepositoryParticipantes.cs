using Microsoft.AspNetCore.Http;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryParticipantes : ApiClientBase, IRepositoryParticipantes
    {
        public RepositoryParticipantes(HttpClient httpClient, IHttpContextAccessor accessor) : base(httpClient, accessor)
        {
        }

        public async Task<List<Participante>> GetByActividadIdAsync(int actividadId)
        {
            return await GetAsync<List<Participante>>($"/apiRandomPay/Participantes/PorActividad/{actividadId}") ?? new List<Participante>();
        }

        public Task<Participante?> GetByIdAsync(int id)
        {
            return GetAsync<Participante>($"/apiRandomPay/Participantes/{id}");
        }

        public Task AddAsync(Participante participante)
        {
            return PostAsync("/apiRandomPay/Participantes", participante);
        }

        public Task UpdateAsync(Participante participante)
        {
            return PutAsync("/apiRandomPay/Participantes", participante);
        }

        public Task DeleteAsync(int id)
        {
            return DeleteAsync($"/apiRandomPay/Participantes/{id}");
        }

        public async Task<bool> ExisteParticipanteEnActividad(int idActividad, string nombre, int? idUsuario)
        {
            var participantes = await GetByActividadIdAsync(idActividad);
            return participantes.Any(p => p.NOMBREPARTICIPANTE.Equals(nombre, StringComparison.OrdinalIgnoreCase) && p.IDUSUARIO == idUsuario);
        }
    }
}
