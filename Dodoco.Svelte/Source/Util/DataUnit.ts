export class DataUnit {

    public static readonly BYTE: number = 1.0;
    public static readonly KILOBYTE: number = 1000.0 * DataUnit.BYTE;
    public static readonly MEGABYTE: number = Math.pow(DataUnit.KILOBYTE, 2.0);
    public static readonly GIGABYTE: number = Math.pow(DataUnit.KILOBYTE, 3.0);
    public static readonly TERABYTE: number = Math.pow(DataUnit.KILOBYTE, 4.0);

}