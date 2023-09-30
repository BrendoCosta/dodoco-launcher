namespace Dodoco.Core.Util.FileSystem {

    public static class DataUnitFormatter {

        private static readonly List<Tuple<string, string>> units = new List<Tuple<string, string>> {

            new Tuple<string, string>("B", "bytes"),
            new Tuple<string, string>("kB", "kilobytes"),
            new Tuple<string, string>("MB", "megabytes"),
            new Tuple<string, string>("GB", "gigabytes"),
            new Tuple<string, string>("TB", "terabytes")

        };

        public static string Format(double bytes) {

            return Format(bytes, DataUnitFormatterOption.USE_SYMBOL);

        }

        public static string Format(double bytes, DataUnitFormatterOption option) {

            double resultNumber = 0.0D;
            string resultString;
            int i;

            for (i = 0; i < units.Count && bytes >= DataUnit.KILOBYTE; i++, bytes /= DataUnit.KILOBYTE) {

                resultNumber = bytes / DataUnit.KILOBYTE;

            }

            resultString = $"{resultNumber:0.##}";

            switch (option) {

                case DataUnitFormatterOption.USE_NONE:
                    break;
                case DataUnitFormatterOption.USE_SYMBOL:
                    resultString += $" {units[i].Item1}";
                    break;
                case DataUnitFormatterOption.USE_FULLNAME:
                    resultString += $" {units[i].Item2}";
                    break;

            }

            return resultString;

        }

    }

}