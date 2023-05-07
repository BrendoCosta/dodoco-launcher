import { DodocoConfig } from "./dodoco-launcher.config.js";
import * as Path from "path";

export default {

    input: [ Path.join(Path.join("./", DodocoConfig.paths.source), "/**/*.{js,ts,svelte}") ],
    output: Path.join(Path.join("./", DodocoConfig.paths.locale), "$LOCALE.yaml"),
    locales: [ "en-US" ],
    lexers: {

        html: [ "HTMLLexer" ],
        ts: [ "JavascriptLexer" ],
        default: [ "JavascriptLexer" ]

    }

}
