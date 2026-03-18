using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;

namespace RandomPayMCSD.Services
{
    // --- 1. NUEVA CLASE PARA MAPEAR QUIÉN PAGA A QUIÉN ---
    public class Transferencia
    {
        public int IdDeudor { get; set; }
        public string NombreDeudor { get; set; }
        public int IdAcreedor { get; set; }
        public string NombreAcreedor { get; set; }
        public double Cantidad { get; set; }
    }

    public class BalanceItem
    {
        public int IdParticipante { get; set; }
        public string Participante { get; set; }
        public double Debe { get; set; } // En positivo si le deben dinero, en negativo si debe
    }

    public class BalanceService
    {
        private const double TOLERANCIA_CENTIMO = 0.01;

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

                // B) Restamos la cantidad individual que cada persona DEBE (según la tabla de repartos)
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

            // Redondeamos y normalizamos residuos de +/- 0,01
            foreach (var b in balances)
            {
                b.Debe = Math.Round(b.Debe, 2, MidpointRounding.AwayFromZero);
                if (Math.Abs(b.Debe) <= TOLERANCIA_CENTIMO)
                {
                    b.Debe = 0;
                }
            }

            // Ordenamos: primero los que tienen saldo positivo (les deben dinero), luego los que deben
            return balances.OrderByDescending(b => b.Debe).ToList();
        }

        // --- 2. NUEVO MÉTODO: ALGORITMO DE DEUDAS CRUZADAS ---
        public async Task<List<Transferencia>> GetTransferenciasAsync(int idActividad)
        {
            // Calculamos los saldos actuales usando el método de arriba
            var balances = await GetBalancesActividadAsync(idActividad);

            // Separamos a los que deben dinero (negativo) y a los que les deben (positivo)
            var deudores = balances.Where(b => b.Debe < -TOLERANCIA_CENTIMO).OrderBy(b => b.Debe).ToList();
            var acreedores = balances.Where(b => b.Debe > TOLERANCIA_CENTIMO).OrderByDescending(b => b.Debe).ToList();

            List<Transferencia> transferencias = new List<Transferencia>();
            int i = 0; // Índice para recorrer deudores
            int j = 0; // Índice para recorrer acreedores

            while (i < deudores.Count && j < acreedores.Count)
            {
                var deudor = deudores[i];
                var acreedor = acreedores[j];

                double deuda = Math.Abs(deudor.Debe);
                double credito = acreedor.Debe;

                // Calculamos cuánto le puede pagar este deudor a este acreedor
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

                // Ajustamos los saldos temporales para la siguiente iteración
                deudor.Debe = Math.Round(deudor.Debe + aPagar, 2, MidpointRounding.AwayFromZero);
                acreedor.Debe = Math.Round(acreedor.Debe - aPagar, 2, MidpointRounding.AwayFromZero);

                if (Math.Abs(deudor.Debe) <= TOLERANCIA_CENTIMO) deudor.Debe = 0;
                if (Math.Abs(acreedor.Debe) <= TOLERANCIA_CENTIMO) acreedor.Debe = 0;

                if (deudor.Debe == 0) i++; // El deudor ya pagó todo lo que debía
                if (acreedor.Debe == 0) j++; // El acreedor ya cobró todo lo que le debían
            }

            return transferencias;
        }

        public async Task<Participante> ElegirPagadorAlAzar(int idActividad)
        {
            List<Participante> participantes = await _repoParticipantes.GetByActividadIdAsync(idActividad);
            if (participantes == null || !participantes.Any()) return null;

            Random rnd = new Random();
            int index = rnd.Next(participantes.Count);
            return participantes[index];
        }
    }
}