import { Nullable } from "@Dodoco/index";
import { LanguageConstants } from "@Dodoco/Language";

import i18next from "i18next";
import HttpBackend from "i18next-http-backend";
import YAML from "yaml";

export class Translator {

    private static instance: Nullable<Translator> = null;

    private constructor() {

        (async function() {

            await i18next.use(HttpBackend).init({
        
                debug: false,
                supportedLngs: LanguageConstants.SupportedLanguages,
                lng: LanguageConstants.DefaultLanguage,
                fallbackLng: LanguageConstants.DefaultLanguage,
                backend: {
        
                    loadPath: LanguageConstants.LanguagesLoadPath,
                    parse: function(data: any) { return YAML.parse(data) },
        
                }
        
            });
        
        })();

    }

    public static GetInstance(): Translator {

        if (Translator.instance == null) {

            Translator.instance = new Translator();

        }

        return Translator.instance;

    }

    public GetText(path: string) {

        return i18next.t(path);

    }

}