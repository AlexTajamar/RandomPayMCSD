namespace RandomPayMCSD.Models
{
    public class ItemCompra
    {
        public int IdItem { get; set; }
        public int IdActividad { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public decimal PrecioEstimado { get; set; }
        public bool Comprado { get; set; }
        public int? IdGasto { get; set; }
        public Actividad? Actividad { get; set; }
        public Gasto? Gasto { get; set; }
    }
}
