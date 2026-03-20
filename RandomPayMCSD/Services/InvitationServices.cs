namespace RandomPayMCSD.Services
{
    public class InvitationService
    {
        public string GenerarCodigoUnico()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}