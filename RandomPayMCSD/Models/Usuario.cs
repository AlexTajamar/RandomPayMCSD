namespace RandomPayMCSD.Models
{
    public class Usuario
    {
        public int IDUSUARIO { get; set; }
        public string NOMBRE { get; set; } = string.Empty;
        public string EMAIL { get; set; } = string.Empty;
        public string PASSWORD { get; set; } = string.Empty;
        public string ROL { get; set; } = "USER";
        public ICollection<Actividad> ActividadesCreadas { get; set; } = new List<Actividad>();
        public ICollection<Participante> Participaciones { get; set; } = new List<Participante>();
    }
}
