using System.Security.Cryptography;
using System.Text;

namespace UltraStock.Helpers
{
    public class HashHelper
    {
        public static string ObtenerHash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto)); //utf8 para reconocer tildes o ñ
                StringBuilder builder = new StringBuilder();

                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
