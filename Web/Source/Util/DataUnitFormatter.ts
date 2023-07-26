import { DataUnit } from "./DataUnit";
import { DataUnitFormatterOption } from "./DataUnitFormatterOption";

export class DataUnitFormatter {

    private static units = [
        ["B", "bytes"],
        ["kB", "kilobytes"],
        ["MB", "megabytes"],
        ["GB", "gigabytes"],
        ["TB", "terabytes"]
    ];

    public static Format(bytes: number, option: DataUnitFormatterOption = DataUnitFormatterOption.USE_SYMBOL): string {

        let resultNumber: number = 0.0;
        let resultString: string;
        let i: number;

        for (i = 0; i < DataUnitFormatter.units.length && bytes >= DataUnit.KILOBYTE; i++, bytes /= DataUnit.KILOBYTE) {

            resultNumber = bytes / DataUnit.KILOBYTE;

        }

        resultString = `${resultNumber.toPrecision(3).padStart(1, "0")}`;

        switch (option) {

            case DataUnitFormatterOption.USE_NONE:
                break;
            case DataUnitFormatterOption.USE_SYMBOL:
                resultString += ` ${DataUnitFormatter.units[i][0]}`;
                break;
            case DataUnitFormatterOption.USE_FULLNAME:
                resultString += ` ${DataUnitFormatter.units[i][1]}`;
                break;

        }

        return resultString;

    }

}