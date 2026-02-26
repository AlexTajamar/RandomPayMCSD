using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("PARTICIPANTES")]
    public class Participante
    {
        [Key]
        [Column("ID_PARTICIPANTE")]
        public int IDPARTICIPANTE { get; set; }

        [Column("ID_ACTIVIDAD")]
        public int IDACTIVIDAD { get; set; }

        [Column("NOMBRE_PARTICIPANTE")]
        public string NOMBREPARTICIPANTE { get; set; }

        [Column("ID_USUARIO")]
        public int? IDUSUARIO { get; set; }

        // Relaciones
        [ForeignKey("IDACTIVIDAD")]
        public Actividad Actividad { get; set; }

        [ForeignKey("IDUSUARIO")]
        public Usuario Usuario { get; set; }

        public ICollection<Gasto> GastosPagados { get; set; }
    }
}