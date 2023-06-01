import { Language } from ".";

export class LanguageConstants {

    public static DefaultLanguage: Language = Language.en_US;
    public static SupportedLanguages: Language[] = Object.values(Language);
    public static LanguagesLoadPath: string = "/Language/{{lng}}.yaml";

}