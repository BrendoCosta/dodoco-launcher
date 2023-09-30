import { Global } from "@Dodoco/index";
import { GameState } from "./Generated/Dodoco/Core/Game/GameState";
import { GameDownloadState } from "@Dodoco/Generated/Dodoco/Core/Game/GameDownloadState";
import { GameIntegrityCheckState } from "@Dodoco/Generated/Dodoco/Core/Game/GameIntegrityCheckState";
import { GameUpdateState } from "@Dodoco/Generated/Dodoco/Core/Game/GameUpdateState";
import { LauncherDependency } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherDependency";
import { WinePackageManagerState } from "@Dodoco/Generated/Dodoco/Core/Wine/WinePackageManagerState";
import { get } from "svelte/store";

// Stylesheets

import { install } from "@twind/core";
import config from "@Dodoco/../twind.config.js";
install(config);
import "@Dodoco/Stylesheet/Style.css";

// Generated types

import { MainController } from "./Generated/Dodoco/Application/Control/MainController";
import { SplashController } from "./Generated/Dodoco/Application/Control/SplashController";
import { GameController } from "./Generated/Dodoco/Application/Control/GameController";
import { LauncherController } from "./Generated/Dodoco/Application/Control/LauncherController";
import { WineController } from "./Generated/Dodoco/Application/Control/WineController";

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
        interpolation: { escapeValue: false },
        backend: {

            loadPath: LanguageConstants.LanguagesLoadPath,
            parse: function(data: any) { return YAML.parse(data) },

        }

    });

    i18next.on("languageChanged", () => {

        Global.i18nInstance.set(i18next);

    });

    i18next.changeLanguage((await LauncherController.GetControllerInstance().GetLauncherSettings()).Launcher.Language);

})()

// Setup view's data update

setInterval(async () => {

    try {

        Global._MainViewData.set(await MainController.GetControllerInstance().GetViewData());
        Global._SplashViewData.set(await SplashController.GetControllerInstance().GetViewData());
        Global._WineControllerViewData.set(await WineController.GetControllerInstance().GetViewData());
        Global._GameState.set(await GameController.GetControllerInstance().GetGameState());
        Global._GameDownloadState.set(await GameController.GetControllerInstance().GetGameDownloadState());
        Global._GameIntegrityCheckState.set(await GameController.GetControllerInstance().GetGameIntegrityCheckState());
        Global._GameUpdateState.set(await GameController.GetControllerInstance().GetGameUpdateState());
        Global._LauncherDependency.set(await LauncherController.GetControllerInstance().GetLauncherDependency());
        Global._LauncherState.set(await LauncherController.GetControllerInstance().GetLauncherState());
        Global._WinePackageManagerState.set(await WineController.GetControllerInstance().GetWinePackageManagerState());
        Global._UiStatesHelpers.set({
            GameIsDownloading: (get(Global._GameDownloadState) ?? GameDownloadState.DOWNLOADED) != GameDownloadState.DOWNLOADED,
            GameIsUpdating: (get(Global._GameUpdateState) ?? GameUpdateState.UPDATED) != GameUpdateState.UPDATED,
            GameIsCheckingIntegrity: (get(Global._GameIntegrityCheckState) ?? GameIntegrityCheckState.IDLE) != GameIntegrityCheckState.IDLE,
            GameIsRunning: (get(Global._GameState) ?? GameState.READY) == GameState.RUNNING,
            WinePackageManagerIsWorking: (get(Global._WinePackageManagerState) ?? WinePackageManagerState.READY) != WinePackageManagerState.READY,
            get LauncherIsBusy() {
                return this.GameIsCheckingIntegrity || this.GameIsDownloading || this.GameIsUpdating || this.GameIsRunning || this.WinePackageManagerIsWorking
            },
            LauncherIsWaiting: (get(Global._LauncherDependency) ?? LauncherDependency.NONE) != LauncherDependency.NONE
        });

    } catch (error: any) {}

}, 500);

// Run

const app: any = new App({

    target: document.body,
    props: { routes }

});

export default app;