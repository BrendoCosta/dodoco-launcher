using System.Text;

namespace Dodoco.Util.Hash {

    public static class MD5 {

        public static string ComputeHash(string buffer) {

            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(buffer));
            return System.Convert.ToHexString(hash);

        }

        public static string ComputeHash(byte[] buffer) {

            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5Hash.ComputeHash(buffer);
            return System.Convert.ToHexString(hash);

        }

    }

}