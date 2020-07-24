using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace T.Web
{
    public static class WebExtensionsMethods
    {
        private static byte[] _key = { 65, 76, 83, 69, 71 };

        public static string Encrypt(this string plainText)
        {
            return HttpServerUtility.UrlTokenEncode(ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), _key, DataProtectionScope.LocalMachine));
        }

        public static string Decrypt(this string text)
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(HttpServerUtility.UrlTokenDecode(text), _key, DataProtectionScope.LocalMachine));
        }
    }
}
