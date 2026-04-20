namespace RandomPayMCSD.Models
{
    public class SeguridadUsuario
    {
        public int IdUsuario { get; set; }
        public string Salt { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public string? TokenRecuperacion { get; set; }
        public DateTime? FechaExpiracionToken { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
