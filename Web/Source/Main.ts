import Router, { push } from "svelte-spa-router";

// Pages

import Main from "./Pages/Main.svelte";
import Splash from "./Pages/Splash.svelte";

// Setup routes

const routes: any = {

    "/splash": Splash

}

// Setup locales

import i18next from "i18next";
import HttpBackend from "i18next-http-backend";
import YAML from "yaml";
import { LocaleConstants } from "./Locale";

(async function() {

    await i18next.use(HttpBackend).init({

        debug: false,
        supportedLngs: LocaleConstants.supportedLocales,
        lng: LocaleConstants.defaultLocale,
        fallbackLng: LocaleConstants.defaultLocale,
        backend: {

            loadPath: LocaleConstants.localesLoadPath,
            parse: function(data: any) { return YAML.parse(data) },

        }

    });

    console.log(i18next.t("test.hello"));
    console.log(i18next.t("test.world"));

})()

// Run

const app: any = new Main({

    target: document.body,
    props: { routes }

});

export default app;