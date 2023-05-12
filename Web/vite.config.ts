import { svelte } from "@sveltejs/vite-plugin-svelte";
import sveltePreprocess from "svelte-preprocess";
import { defineConfig } from "vite";
import { DodocoConfig } from "./dodoco-launcher.config.js";
import * as Path from "path";

let projectRootPath = "./";

export default defineConfig({
	plugins: [
		svelte({
			preprocess: sveltePreprocess()
		})
	],
	root: projectRootPath,
	publicDir: Path.join(projectRootPath, DodocoConfig.paths.static),
	build: {
		outDir: Path.join(projectRootPath, DodocoConfig.paths.build)
	},
	server: {
		fs: {
			allow: [ projectRootPath ]
		}
	},
	resolve:{
		alias:{
			"@Dodoco" : Path.resolve(__dirname, DodocoConfig.paths.source)
		}
	}
});
