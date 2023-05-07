import { Locale } from "./";

export class LocaleConstants {

    public static defaultLocale: Locale = Locale.enUS;
    public static supportedLocales: Locale[] = [
        Locale.enUS
    ]
    public static localesLoadPath: string = "/Locale/{{lng}}.yaml";

}