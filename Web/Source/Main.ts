import { Nullable, GlobalInstances } from "@Dodoco/index";
import { RpcClient } from "@Dodoco/Backend";
import { onMount } from "svelte";

// Stylesheets
import { install } from "@twind/core";
import config from "@Dodoco/../twind.config.js";
install(config);
import "@Dodoco/Stylesheet/Style.css";

// Generated types

import { LauncherController } from "./Generated/Dodoco/Network/Controller/LauncherController";

// Pages

import Main from "./Pages/Main.svelte";
import Splash from "./Pages/Splash.svelte";
import Launcher from "./Pages/Launcher.svelte";

// Setup routes

const routes: any = {

    "/": Main,
    "/Splash": Splash,
    "/Launcher": Launcher

}

// Setup locales

import i18next from "i18next";
import HttpBackend from "i18next-http-backend";
import YAML from "yaml";
import { LanguageConstants } from "./Language";

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

    i18next.on("languageChanged", () => {

        GlobalInstances.i18nInstance.set(i18next);

    });

    i18next.changeLanguage((await LauncherController.GetInstance().GetLauncherSettings()).launcher.language);

})()

setInterval(async () => {

    GlobalInstances.LauncherInstance.set(await LauncherController.GetInstance().GetEntityInstance());
    GlobalInstances.GameInstance.set(await LauncherController.GetInstance().GetGame());

}, 500);

// Run

const app: any = new Main({

    target: document.body,
    props: { routes }

});

export default app;