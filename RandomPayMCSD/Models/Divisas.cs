using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("DIVISAS")]
    public class Divisa
    {
        [Key]
        [Column("IDDIVISA")]
        public int IdDivisa { get; set; }

        [Column("CODIGO")]
        public string Codigo { get; set; }

        [Column("TASA")]
        public double Tasa { get; set; }
    }
}