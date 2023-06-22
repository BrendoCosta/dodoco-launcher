import { Global } from "@Dodoco/index";

// Stylesheets

import { install } from "@twind/core";
import config from "@Dodoco/../twind.config.js";
install(config);
import "@Dodoco/Stylesheet/Style.css";

// Generated types

import { MainController } from "./Generated/Dodoco/Application/Control/MainController";
import { SplashController } from "./Generated/Dodoco/Application/Control/SplashController";
import { SettingsController } from "./Generated/Dodoco/Application/Control/SettingsController";

// Pages

import App from "./Pages/App.svelte";
import Main from "./Pages/Main.svelte";
import Splash from "./Pages/Splash.svelte";

// Setup routes

const routes: any = {

    "/": App,
    "/Main": Main,
    "/Splash": Splash

}

// Setup languages

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

        Global.i18nInstance.set(i18next);

    });

    i18next.changeLanguage((await SettingsController.GetControllerInstance().GetLauncherSettings()).Launcher.Language);

})()

// Setup view's data update

setInterval(async () => {

    try {

        Global._MainViewData.set(await MainController.GetControllerInstance().GetViewData());
        Global._SettingsViewData.set(await SettingsController.GetControllerInstance().GetViewData());
        Global._SplashViewData.set(await SplashController.GetControllerInstance().GetViewData());

    } catch (error: any) {}

}, 500);

// Run

const app: any = new App({

    target: document.body,
    props: { routes }

});

export default app;