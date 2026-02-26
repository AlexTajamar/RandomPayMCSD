using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int IDUSUARIO { get; set; }

        [Column("NOMBRE")]
        public string NOMBRE { get; set; }

        [Column("EMAIL")]
        public string EMAIL { get; set; }

        [Column("PASSWORD")]
        public string PASSWORD { get; set; }

        [Column("ROL")]
        public string ROL { get; set; }

        // Propiedades de navegación
        public ICollection<Actividad> ActividadesCreadas { get; set; }
        public ICollection<Participante> Participaciones { get; set; }
    }
}