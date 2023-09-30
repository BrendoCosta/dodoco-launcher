namespace Dodoco.Core.Util.FileSystem {

    public static class DataUnit {

        public static readonly double BYTE = 1.0D;
        public static readonly double KILOBYTE = 1000.0D * BYTE;
        public static readonly double MEGABYTE = Convert.ToUInt64(Math.Pow(KILOBYTE, 2.0D));
        public static readonly double GIGABYTE = Convert.ToUInt64(Math.Pow(KILOBYTE, 3.0D));
        public static readonly double TERABYTE = Convert.ToUInt64(Math.Pow(KILOBYTE, 4.0D));

    }

}