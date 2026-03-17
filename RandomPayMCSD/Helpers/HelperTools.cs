namespace RandomPayMCSD.Helpers
{
    public class HelperTools
    {
        //Importante, nuestro Salt debe ser exacto al tamaño del campo de la BBDD
        public static string GenerateSalt()
        {
            Random random = new Random();
            string salt = "";
            for (int x = 1; x <= 50; x++)
            {
                int num = random.Next(1, 255);
                char letra = Convert.ToChar(num);
                salt += letra;
            }
            return salt;
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
