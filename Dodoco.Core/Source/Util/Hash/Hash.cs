using System.Security.Cryptography;
using System.Text;

namespace Dodoco.Core.Util.Hash {

    public class Hash {

        public HashAlgorithm Algorithm { get; private set; }

        public Hash(HashAlgorithm algorithm) {

            this.Algorithm = algorithm;

        }

        public string ComputeHash(string filePath) {

            using (FileStream stream = File.OpenRead(filePath)) {

                byte[] hash = this.Algorithm.ComputeHash(stream);
                return Convert.ToHexString(hash);

            }

        }

        public string ComputeHash(string text, Encoding encoding) {

            byte[] hash = this.Algorithm.ComputeHash(encoding.GetBytes(text));
            return Convert.ToHexString(hash);

        }

        public string ComputeHash(byte[] buffer) {

            byte[] hash = this.Algorithm.ComputeHash(buffer);
            return Convert.ToHexString(hash);

        }

    }

}