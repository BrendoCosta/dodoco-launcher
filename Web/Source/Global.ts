import { writable } from "svelte/store";
import i18next from "i18next";
// Generated types
import { MainViewData } from "@Dodoco/Generated/Dodoco/Application/Control/MainViewData";
import { SettingsViewData } from "./Generated/Dodoco/Application/Control/SettingsViewData";
import { SplashViewData } from "@Dodoco/Generated/Dodoco/Application/Control/SplashViewData";

export const _MainViewData = writable({} as MainViewData);
export const _SettingsViewData = writable({} as SettingsViewData);
export const _SplashViewData = writable({} as SplashViewData);
export const i18nInstance = writable(i18next);