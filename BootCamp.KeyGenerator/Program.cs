using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.KeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                var APIKey = Convert.ToBase64String(secretKeyByteArray);
                var AppId = Guid.NewGuid();
                Debug.WriteLine(APIKey);
                Debug.WriteLine(AppId);
            }
        }
    }
}
