namespace RandomPayMCSD.Models
{
    public class Actividad
    {
        public int IDACTIVIDAD { get; set; }
        public string NOMBREACTIVIDAD { get; set; } = string.Empty;
        public int IDCREADOR { get; set; }
        public string MONEDAPRINCIPAL { get; set; } = "EUR";
        public string INVITACIONCOD { get; set; } = string.Empty;
        public DateTime FECHACREACION { get; set; }
        public Usuario? Creador { get; set; }
        public string? IMAGEN { get; set; }
        public ICollection<Participante> Participantes { get; set; } = new List<Participante>();
        public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
    }
}
