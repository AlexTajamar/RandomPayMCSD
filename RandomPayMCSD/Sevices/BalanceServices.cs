using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RandomPayMCSD.Services
{
    // Esta clase nos sirve para pasar los datos calculados a la vista
    public class BalanceItem
    {
        public string Participante { get; set; }
        public decimal Debe { get; set; }
        // Si Debe es Positivo (+): Ha pagado de más, le deben dinero.
        // Si Debe es Negativo (-): Ha pagado de menos, debe dinero al grupo.
    }

    public class BalanceService
    {
        private readonly IRepositoryActividades _repoActividades;

        public BalanceService(IRepositoryActividades repoActividades)
        {
            _repoActividades = repoActividades;
        }

        // 1. CÁLCULO ESTILO TRICOUNT
        public async Task<List<BalanceItem>> GetBalancesActividadAsync(int idActividad)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(idActividad);

            if (actividad == null || actividad.Participantes == null || !actividad.Participantes.Any())
            {
                return new List<BalanceItem>();
            }

            int totalParticipantes = actividad.Participantes.Count;
            // Sumamos todos los gastos. Si no hay, es 0.
            decimal totalGastos = actividad.Gastos?.Sum(g => g.IMPORTE) ?? 0;
            // Lo que debería haber pagado cada uno
            decimal cuotaPorPersona = totalParticipantes > 0 ? totalGastos / totalParticipantes : 0;

            List<BalanceItem> balances = new List<BalanceItem>();

            foreach (var participante in actividad.Participantes)
            {
                // ¿Cuánto ha puesto de su bolsillo esta persona?
                decimal totalPagadoPorPersona = actividad.Gastos?
                    .Where(g => g.IDPAGADOR == participante.IDPARTICIPANTE)
                    .Sum(g => g.IMPORTE) ?? 0;

                // Su balance final = Lo que puso - Lo que le tocaba poner
                decimal balanceFinal = totalPagadoPorPersona - cuotaPorPersona;

                balances.Add(new BalanceItem
                {
                    Participante = participante.NOMBREPARTICIPANTE,
                    Debe = Math.Round(balanceFinal, 2)
                });
            }

            // Devolvemos la lista ordenada: primero los que tienen saldo a favor
            return balances.OrderByDescending(b => b.Debe).ToList();
        }

        // 2. FUNCIONALIDAD RULETA RANDOM 
        public async Task<Participante> ElegirPagadorAlAzar(int idActividad)
        {
            Actividad actividad = await _repoActividades.GetByIdWithDetailsAsync(idActividad);

            if (actividad == null || actividad.Participantes == null || !actividad.Participantes.Any())
                return null;

            // Creamos un random y elegimos un índice de la lista de participantes
            Random rnd = new Random();
            int indexMalaSuerte = rnd.Next(actividad.Participantes.Count);

            return actividad.Participantes.ElementAt(indexMalaSuerte);
        }
    }
}