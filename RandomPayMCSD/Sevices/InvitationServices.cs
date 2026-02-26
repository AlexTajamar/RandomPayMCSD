namespace RandomPayMCSD.Services
{
    public class InvitationService
    {
        // Genera un código único de invitación (p.ej., usando GUID)
        public string GenerarCodigoUnico()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}