/** @type {import('tailwindcss').Config} */

import { DodocoConfig } from "./dodoco-launcher.config.js";
import * as Path from "path";
import colors from "tailwindcss/colors";

export default {
    content: [
        Path.join(__dirname, DodocoConfig.paths.source, "**/*.{html,js,svelte,ts}"),
        "./node_modules/flowbite-svelte/**/*.{html,js,svelte,ts}"
    ],
    darkMode: "class",
    theme: {
        colors: Object.assign(colors, {
            "darkgray": "#393b40",
            "lightgray": "#78797b",
            "golden": "#dcbc60",
            "lightgolden": "#e6d6a8"
        })
    }
}

