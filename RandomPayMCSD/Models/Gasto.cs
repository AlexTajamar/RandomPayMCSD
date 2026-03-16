using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("GASTOS")]
    public class Gasto
    {
        [Key]
        [Column("ID_GASTO")]
        public int IDGASTO { get; set; }

        [Column("ID_ACTIVIDAD")]
        public int IDACTIVIDAD { get; set; }

        [Column("ID_PAGADOR")]
        public int IDPAGADOR { get; set; }

        [Column("CONCEPTO")]
        public string CONCEPTO { get; set; }

        [Column("IMPORTE")]
        public decimal IMPORTE { get; set; }

        [Column("FECHA")]
        public DateTime FECHA { get; set; }

        // Relaciones
        [ForeignKey("IDACTIVIDAD")]
        public Actividad Actividad { get; set; }

        [ForeignKey("IDPAGADOR")]
        public Participante Pagador { get; set; }
        public List<RepartoGasto> Repartos { get; set; }
    }
}