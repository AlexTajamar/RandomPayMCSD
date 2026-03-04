using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("REPARTOS_GASTO")]
    public class RepartoGasto
    {
        [Key]
        [Column("IDREPARTO")]
        public int IdReparto { get; set; }

        [Column("IDGASTO")]
        public int IdGasto { get; set; }

        [Column("IDPARTICIPANTE")]
        public int IdParticipante { get; set; }

        [Column("CANTIDAD")]
        public double Cantidad { get; set; }
    }
}