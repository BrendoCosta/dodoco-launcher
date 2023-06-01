namespace Dodoco.Util.Text {

    public static class TextUtil {

        public static string StringToUTF8(string text) {

            return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(text));

        }

        public static byte[] UTF8StringToUTF8Bytes(string text) {

            return System.Text.Encoding.UTF8.GetBytes(text);

        }

        /*
         * By JLopez and tedebus
         * https://stackoverflow.com/a/44244610
        */
        public static string FormatBytes(long bytes, bool useUnit = false) {

            string[] suffix = { "B", "kB", "MB", "GB", "TB" };
            double dblSByte = 0D;
            int i;
            
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024) {

                dblSByte = bytes / 1024.0;

            }

            return $"{dblSByte:0.##} {( useUnit ? suffix[i] : null )}";

        }

    }

}