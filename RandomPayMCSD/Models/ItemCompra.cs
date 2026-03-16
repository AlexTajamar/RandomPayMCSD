using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("LISTA_COMPRA")]
    public class ItemCompra
    {
        [Key]
        [Column("ID_ITEM")]
        public int IdItem { get; set; }

        [Column("ID_ACTIVIDAD")]
        public int IdActividad { get; set; }

        [Column("NOMBRE_ITEM")]
        public string NombreItem { get; set; }

        [Column("PRECIO_ESTIMADO")]
        public decimal PrecioEstimado { get; set; }

        [Column("COMPRADO")]
        public bool Comprado { get; set; }

        [Column("ID_GASTO")]
        public int? IdGasto { get; set; }

        [ForeignKey("IdActividad")]
        public Actividad Actividad { get; set; }

        [ForeignKey("IdGasto")]
        public Gasto Gasto { get; set; }
    }
}