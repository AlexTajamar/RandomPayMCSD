namespace RandomPayMCSD.Models
{
    public class Gasto
    {
        public int IDGASTO { get; set; }
        public int IDACTIVIDAD { get; set; }
        public int IDPAGADOR { get; set; }
        public string CONCEPTO { get; set; } = string.Empty;
        public decimal IMPORTE { get; set; }
        public DateTime FECHA { get; set; }
        public Actividad? Actividad { get; set; }
        public Participante? Pagador { get; set; }
        public List<RepartoGasto> Repartos { get; set; } = new();
    }
}
