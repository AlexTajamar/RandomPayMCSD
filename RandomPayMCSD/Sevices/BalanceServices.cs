using RandomPayMCSD.Models;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.ViewModels;

namespace RandomPayMCSD.Services
{
    public class BalanceService
    {
        private readonly IRepositoryActividades _repoActividades;

        public BalanceService(IRepositoryActividades repoActividades)
        {
            _repoActividades = repoActividades;
        }

        public async Task<List<BalanceParticipante>> CalcularBalancesAsync(int actividadId)
        {
            var actividad = await _repoActividades.GetByIdWithDetailsAsync(actividadId);
            if (actividad == null) return new List<BalanceParticipante>();

            var participantes = actividad.Participantes.ToList();
            var gastos = actividad.Gastos.ToList();

            if (!participantes.Any() || !gastos.Any())
                return participantes.Select(p => new BalanceParticipante
                {
                    Nombre = p.NOMBREPARTICIPANTE,
                    DebeRecibir = 0
                }).ToList();

            // 1. Calcular total pagado por cada participante
            var pagado = participantes.ToDictionary(p => p.IDPARTICIPANTE, p => 0m);
            foreach (var gasto in gastos)
            {
                pagado[gasto.IDPAGADOR] += gasto.IMPORTE;
            }

            // 2. Media por persona
            var totalGastos = gastos.Sum(g => g.IMPORTE);
            var media = totalGastos / participantes.Count;

            // 3. Balance neto: media - pagado (positivo = debe recibir, negativo = debe pagar)
            var balances = new List<BalanceParticipante>();
            foreach (var p in participantes)
            {
                var debe = media - pagado[p.IDPARTICIPANTE];
                balances.Add(new BalanceParticipante
                {
                    Nombre = p.NOMBREPARTICIPANTE,
                    DebeRecibir = debe
                });
            }

            return balances;
        }

        // Método opcional para simplificar deudas (algoritmo greedy)
        public List<TransaccionSimplificada> SimplificarDeudas(List<BalanceParticipante> balances)
        {
            var deudores = balances.Where(b => b.DebeRecibir < 0)
                                   .OrderBy(b => b.DebeRecibir)
                                   .ToList();
            var acreedores = balances.Where(b => b.DebeRecibir > 0)
                                     .OrderByDescending(b => b.DebeRecibir)
                                     .ToList();

            var transacciones = new List<TransaccionSimplificada>();

            int i = 0, j = 0;
            while (i < deudores.Count && j < acreedores.Count)
            {
                var deudor = deudores[i];
                var acreedor = acreedores[j];

                var cantidad = Math.Min(-deudor.DebeRecibir, acreedor.DebeRecibir);

                transacciones.Add(new TransaccionSimplificada
                {
                    Deudor = deudor.Nombre,
                    Acreedor = acreedor.Nombre,
                    Cantidad = cantidad
                });

                deudor.DebeRecibir += cantidad;
                acreedor.DebeRecibir -= cantidad;

                if (deudor.DebeRecibir == 0) i++;
                if (acreedor.DebeRecibir == 0) j++;
            }

            return transacciones;
        }
    }
}