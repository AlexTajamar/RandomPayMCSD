namespace RandomPayMCSD.Models
{
    public class Participante
    {
        public int IDPARTICIPANTE { get; set; }
        public int IDACTIVIDAD { get; set; }
        public string NOMBREPARTICIPANTE { get; set; } = string.Empty;
        public int? IDUSUARIO { get; set; }
        public Actividad? Actividad { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<Gasto> GastosPagados { get; set; } = new List<Gasto>();
    }
}
