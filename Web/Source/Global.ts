import { Writable, writable } from "svelte/store";
import i18next from "i18next";
// Generated types
import { MainViewData } from "@Dodoco/Generated/Dodoco/Application/Control/MainViewData";
import { SplashViewData } from "@Dodoco/Generated/Dodoco/Application/Control/SplashViewData";
import { WineControllerViewData } from "@Dodoco/Generated/Dodoco/Application/Control/WineControllerViewData";
import { Nullable, CommonErrorData } from "./";
import { GameState } from "./Generated/Dodoco/Core/Game/GameState";
import { LauncherDependency } from "./Generated/Dodoco/Core/Launcher/LauncherDependency";
import { LauncherState } from "./Generated/Dodoco/Core/Launcher/LauncherState";
import { WinePackageManagerState } from "./Generated/Dodoco/Core/Wine/WinePackageManagerState";

export const _AppError: Writable<CommonErrorData[]> = writable([] as CommonErrorData[]);
export const _MainViewData = writable({} as MainViewData);
export const _SplashViewData = writable({} as SplashViewData);
export const _WineControllerViewData = writable({} as WineControllerViewData);
export const _GameState = writable(null as Nullable<GameState>);
export const _LauncherDependency = writable(null as Nullable<LauncherDependency>);
export const _LauncherState = writable(null as Nullable<LauncherState>);
export const _WinePackageManagerState = writable(null as Nullable<WinePackageManagerState>);
export const i18nInstance = writable(i18next);