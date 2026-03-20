using System.Security.Cryptography;

namespace RandomPayMCSD.Helpers
{
    public class HelperTools
    {
        public static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32); 
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
