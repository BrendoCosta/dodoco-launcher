namespace Dococo.Util.Text {

    public static class StringUtil {

        public static string StringToUTF8(string text) {

            return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(text));

        }

        public static byte[] UTF8StringToUTF8Bytes(string text) {

            return System.Text.Encoding.UTF8.GetBytes(text);

        }

    }

}