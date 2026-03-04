using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Services
{
    public class BalanceItem
    {
        public int IdParticipante { get; set; }
        public string Participante { get; set; }
        public double Debe { get; set; } // En positivo si le deben dinero, en negativo si debe
    }

    public class BalanceService
    {
        private IRepositoryGastos _repoGastos;
        private IRepositoryParticipantes _repoParticipantes;
        private IRepositoryRepartos _repoRepartos;

        public BalanceService(
            IRepositoryGastos repoGastos,
            IRepositoryParticipantes repoParticipantes,
            IRepositoryRepartos repoRepartos)
        {
            _repoGastos = repoGastos;
            _repoParticipantes = repoParticipantes;
            _repoRepartos = repoRepartos;
        }

        public async Task<List<BalanceItem>> GetBalancesActividadAsync(int idActividad)
        {
            // 1. Obtenemos participantes, gastos y todos los repartos asociados a la actividad
            List<Participante> participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);

            // AQUÍ ESTABA EL ERROR: Usamos tu método GetByActividadIdAsync
            List<Gasto> gastos = await _repoGastos.GetByActividadIdAsync(idActividad);

            List<BalanceItem> balances = new List<BalanceItem>();

            // Inicializamos el balance a 0 para cada persona
            foreach (var p in participantes)
            {
                balances.Add(new BalanceItem
                {
                    IdParticipante = p.IDPARTICIPANTE,
                    Participante = p.NOMBREPARTICIPANTE,
                    Debe = 0
                });
            }

            // 2. Calculamos los balances reales leyendo de los gastos y los repartos
            foreach (var gasto in gastos)
            {
                // A) Sumamos el importe total al que PAGÓ (porque se lo deben a él)
                var pagador = balances.FirstOrDefault(b => b.IdParticipante == gasto.IDPAGADOR);
                if (pagador != null)
                {
                    pagador.Debe += (double)gasto.IMPORTE;
                }

                // B) Restamos la cantidad individual que cada persona DEBE (según la nueva tabla de repartos)
                var repartosDelGasto = await _repoRepartos.GetRepartosByGastoAsync(gasto.IDGASTO);

                foreach (var reparto in repartosDelGasto)
                {
                    var deudor = balances.FirstOrDefault(b => b.IdParticipante == reparto.IdParticipante);
                    if (deudor != null)
                    {
                        // Se lo restamos, porque es dinero que él "consumió"
                        deudor.Debe -= reparto.Cantidad;
                    }
                }
            }

            // Redondeamos todo a 2 decimales para que se vea limpio
            foreach (var b in balances)
            {
                b.Debe = Math.Round(b.Debe, 2);
            }

            // Ordenamos: primero los que tienen saldo positivo (les deben dinero), luego los que deben
            return balances.OrderByDescending(b => b.Debe).ToList();
        }

        public async Task<Participante> ElegirPagadorAlAzar(int idActividad)
        {
            // Mantenemos la lógica de la ruleta igual
            List<Participante> participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            if (participantes == null || !participantes.Any()) return null;

            Random rnd = new Random();
            int index = rnd.Next(participantes.Count);
            return participantes[index];
        }
    }
}