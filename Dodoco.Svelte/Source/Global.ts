import { Writable, writable, get } from "svelte/store";
import i18next from "i18next";
// Generated types
import { MainViewData } from "@Dodoco/Generated/Dodoco/Application/Control/MainViewData";
import { SplashViewData } from "@Dodoco/Generated/Dodoco/Application/Control/SplashViewData";
import { WineControllerViewData } from "@Dodoco/Generated/Dodoco/Application/Control/WineControllerViewData";
import { Nullable, CommonErrorData } from "./";
import { GameState } from "./Generated/Dodoco/Core/Game/GameState";
import { GameDownloadState } from "./Generated/Dodoco/Core/Game/GameDownloadState";
import { GameIntegrityCheckState } from "./Generated/Dodoco/Core/Game/GameIntegrityCheckState";
import { GameUpdateState } from "./Generated/Dodoco/Core/Game/GameUpdateState";
import { LauncherDependency } from "./Generated/Dodoco/Core/Launcher/LauncherDependency";
import { LauncherState } from "./Generated/Dodoco/Core/Launcher/LauncherState";
import { WinePackageManagerState } from "./Generated/Dodoco/Core/Wine/WinePackageManagerState";

interface ConfirmPopupParams {

    text: string,
    callback: (e: boolean) => void;

}

interface UiStatesHelpers {

    GameIsDownloading: boolean,
    GameIsUpdating: boolean,
    GameIsCheckingIntegrity: boolean,
    GameIsRunning: boolean,
    WinePackageManagerIsWorking: boolean,
    get LauncherIsBusy(): boolean,
    LauncherIsWaiting: boolean

}

export const _AppError: Writable<CommonErrorData[]> = writable([] as CommonErrorData[]);
export const _ConfirmPopup: Writable<ConfirmPopupParams[]> = writable([] as ConfirmPopupParams[]);
export const _MainViewData = writable({} as MainViewData);
export const _SplashViewData = writable({} as SplashViewData);
export const _WineControllerViewData = writable({} as WineControllerViewData);
export const _GameState = writable(null as Nullable<GameState>);
export const _GameDownloadState = writable(null as Nullable<GameDownloadState>);
export const _GameUpdateState = writable(null as Nullable<GameUpdateState>);
export const _GameIntegrityCheckState = writable(null as Nullable<GameIntegrityCheckState>);
export const _LauncherDependency = writable(null as Nullable<LauncherDependency>);
export const _LauncherState = writable(null as Nullable<LauncherState>);
export const _WinePackageManagerState = writable(null as Nullable<WinePackageManagerState>);
export const _UiStatesHelpers = writable({
    GameIsDownloading: false,
    GameIsUpdating: false,
    GameIsCheckingIntegrity: false,
    GameIsRunning: false,
    WinePackageManagerIsWorking: false,
    get LauncherIsBusy () { return false },
    LauncherIsWaiting: false
} as UiStatesHelpers);
export const i18nInstance = writable(i18next);