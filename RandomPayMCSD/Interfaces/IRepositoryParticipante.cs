using RandomPayMCSD.Models;

namespace RandomPayMCSD.Repositories.Interfaces
{
    public interface IRepositoryParticipantes
    {
        Task<List<Participante>> GetByActividadIdAsync(int actividadId);
        Task<Participante?> GetByIdAsync(int id);
        Task AddAsync(Participante participante);
        Task DeleteAsync(int id);
        Task<bool> ExisteParticipanteEnActividad(int actividadId, string nombre, int? idUsuario);
    }
}