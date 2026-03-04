using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Repositories
{
    public class RepositoryDivisas : IRepositoryDivisas
    {
        private RandomPayContext context;

        public RepositoryDivisas(RandomPayContext context)
        {
            this.context = context;
        }

        public async Task<List<Divisa>> GetDivisasAsync()
        {
            return await this.context.Divisas.ToListAsync();
        }

        public async Task<Divisa> GetDivisaByCodigoAsync(string codigo)
        {
            return await this.context.Divisas.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }
    }
}