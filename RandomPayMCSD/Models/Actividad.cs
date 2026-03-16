using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("ACTIVIDADES")]
    public class Actividad
    {
        [Key]
        [Column("ID_ACTIVIDAD")]
        public int IDACTIVIDAD { get; set; }

        [Column("NOMBRE_ACTIVIDAD")]
        public string NOMBREACTIVIDAD { get; set; }

        [Column("ID_CREADOR")]
        public int IDCREADOR { get; set; }

        [Column("MONEDA_PRINCIPAL")]
        public string MONEDAPRINCIPAL { get; set; }

        [Column("INVITACION_COD")]
        public string INVITACIONCOD { get; set; }

        [Column("FECHA_CREACION")]
        public DateTime FECHACREACION { get; set; }

        [ForeignKey("IDCREADOR")]
        public Usuario Creador { get; set; }

        [Column("IMAGEN")]
        public string? IMAGEN { get; set; }

        public ICollection<Participante> Participantes { get; set; }
        public ICollection<Gasto> Gastos { get; set; }
    }
}