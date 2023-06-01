import { DodocoConfig } from "./dodoco-launcher.config.js";
import { LanguageConstants } from "@Dodoco/Language/LanguageConstants.js";
import * as Path from "path";

export default {

    input: [ Path.join(Path.join("./", DodocoConfig.paths.source), "/**/*.{js,ts,svelte}") ],
    output: Path.join(Path.join("./", DodocoConfig.paths.locale), "$LOCALE.yaml"),
    locales: LanguageConstants.SupportedLanguages.map((value) => value.toString()),
    lexers: {

        html: [ "HTMLLexer" ],
        ts: [ "JavascriptLexer" ],
        default: [ "JavascriptLexer" ]

    }

}
