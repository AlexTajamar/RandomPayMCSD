using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPayMCSD.Models
{
    [Table("SEGURIDAD_USUARIOS")]
    public class SeguridadUsuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int IdUsuario { get; set; }

        [Column("SALT")]
        public string Salt { get; set; }

        [Column("PASSWORD_HASH")]
        public byte[] PasswordHash { get; set; }
        [Column("TOKEN_RECUPERACION")]
        public string? TokenRecuperacion { get; set; }

        [Column("TOKEN_EXPIRACION")]
        public DateTime? FechaExpiracionToken { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }
    }
}