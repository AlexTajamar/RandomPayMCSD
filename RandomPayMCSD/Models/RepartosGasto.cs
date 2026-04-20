namespace RandomPayMCSD.Models
{
    public class RepartoGasto
    {
        public int IdReparto { get; set; }
        public int IdGasto { get; set; }
        public int IdParticipante { get; set; }
        public double Cantidad { get; set; }
        public Gasto? Gasto { get; set; }
    }
}
