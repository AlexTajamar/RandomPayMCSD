namespace RandomPayMCSD.Services
{
    public class InvitationService
    {
        public string GenerarCodigoUnico() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
