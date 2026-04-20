using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Services
{
    public class Transferencia
    {
        public int IdDeudor { get; set; }
        public string NombreDeudor { get; set; } = string.Empty;
        public int IdAcreedor { get; set; }
        public string NombreAcreedor { get; set; } = string.Empty;
        public double Cantidad { get; set; }
    }

    public class BalanceItem
    {
        public int IdParticipante { get; set; }
        public string Participante { get; set; } = string.Empty;
        public double Debe { get; set; }
    }

    public class BalanceService
    {
        private const double TOLERANCIA_CENTIMO = 0.01;
        private readonly IRepositoryGastos _repoGastos;
        private readonly IRepositoryParticipantes _repoParticipantes;
        private readonly IRepositoryRepartos _repoRepartos;

        public BalanceService(IRepositoryGastos repoGastos, IRepositoryParticipantes repoParticipantes, IRepositoryRepartos repoRepartos)
        {
            _repoGastos = repoGastos;
            _repoParticipantes = repoParticipantes;
            _repoRepartos = repoRepartos;
        }

        public async Task<List<BalanceItem>> GetBalancesActividadAsync(int idActividad)
        {
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            var gastos = await _repoGastos.GetByActividadIdAsync(idActividad);

            var balances = participantes.Select(p => new BalanceItem
            {
                IdParticipante = p.IDPARTICIPANTE,
                Participante = p.NOMBREPARTICIPANTE,
                Debe = 0
            }).ToList();

            foreach (var gasto in gastos)
            {
                var pagador = balances.FirstOrDefault(b => b.IdParticipante == gasto.IDPAGADOR);
                if (pagador != null) pagador.Debe += (double)gasto.IMPORTE;

                var repartos = await _repoRepartos.GetRepartosByGastoAsync(gasto.IDGASTO);
                foreach (var reparto in repartos)
                {
                    var deudor = balances.FirstOrDefault(b => b.IdParticipante == reparto.IdParticipante);
                    if (deudor != null) deudor.Debe -= reparto.Cantidad;
                }
            }

            foreach (var b in balances)
            {
                b.Debe = Math.Round(b.Debe, 2, MidpointRounding.AwayFromZero);
                if (Math.Abs(b.Debe) <= TOLERANCIA_CENTIMO) b.Debe = 0;
            }

            return balances.OrderByDescending(x => x.Debe).ToList();
        }

        public async Task<List<Transferencia>> GetTransferenciasAsync(int idActividad)
        {
            var balances = await GetBalancesActividadAsync(idActividad);
            var deudores = balances.Where(b => b.Debe < -TOLERANCIA_CENTIMO).OrderBy(b => b.Debe).ToList();
            var acreedores = balances.Where(b => b.Debe > TOLERANCIA_CENTIMO).OrderByDescending(b => b.Debe).ToList();

            var transferencias = new List<Transferencia>();
            int i = 0, j = 0;

            while (i < deudores.Count && j < acreedores.Count)
            {
                var deudor = deudores[i];
                var acreedor = acreedores[j];

                double deuda = Math.Abs(deudor.Debe);
                double credito = acreedor.Debe;
                double aPagar = Math.Round(Math.Min(deuda, credito), 2, MidpointRounding.AwayFromZero);

                if (aPagar > TOLERANCIA_CENTIMO)
                {
                    transferencias.Add(new Transferencia
                    {
                        IdDeudor = deudor.IdParticipante,
                        NombreDeudor = deudor.Participante,
                        IdAcreedor = acreedor.IdParticipante,
                        NombreAcreedor = acreedor.Participante,
                        Cantidad = aPagar
                    });
                }

                deudor.Debe = Math.Round(deudor.Debe + aPagar, 2, MidpointRounding.AwayFromZero);
                acreedor.Debe = Math.Round(acreedor.Debe - aPagar, 2, MidpointRounding.AwayFromZero);

                if (Math.Abs(deudor.Debe) <= TOLERANCIA_CENTIMO) i++;
                if (Math.Abs(acreedor.Debe) <= TOLERANCIA_CENTIMO) j++;
            }

            return transferencias;
        }

        public async Task<Participante?> ElegirPagadorAlAzar(int idActividad)
        {
            var participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            if (!participantes.Any()) return null;

            var rnd = new Random();
            return participantes[rnd.Next(participantes.Count)];
        }
    }
}
