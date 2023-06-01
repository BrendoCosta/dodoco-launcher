using System.Text;

namespace Dodoco.Util.Hash {

    public static class MD5 {

        public static string ComputeHash(string buffer) {

            using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create()) {

                byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(buffer));
                return System.Convert.ToHexString(hash);

            }

        }

        public static string ComputeHash(byte[] buffer) {

            using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create()) {

                byte[] hash = md5Hash.ComputeHash(buffer);
                return System.Convert.ToHexString(hash);

            }

        }

        public static string ComputeHash(FileInfo file) {

            if (!file.Exists) throw new Exception($"The given file doesn't exist");

            using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create()) {

                using (FileStream stream = file.OpenRead()) {

                    byte[] hash = md5Hash.ComputeHash(stream);
                    return System.Convert.ToHexString(hash);

                }

            }

        }

    }

}