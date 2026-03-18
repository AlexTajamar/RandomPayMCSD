using System.Security.Cryptography;

namespace RandomPayMCSD.Helpers
{
    public class HelperTools
    {
        // Salt seguro (Base64) y compatible con NVARCHAR(50)
        public static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32); // 32 bytes => 44 chars base64
            string salt = Convert.ToBase64String(saltBytes);
            return salt.Length > 50 ? salt.Substring(0, 50) : salt;
        }

        public static bool CompareArrays(byte[] a, byte[] b)
        {
            bool iguales = true;
            if(a.Length != b.Length)
            {
                iguales = false;
            }
            else
            {
                //Comparamos byte a byte
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Equals(b[x]) == false)
                    {
                        iguales = false;
                        break;
                    }
                }
            }
            return iguales;
        }
    }
}
