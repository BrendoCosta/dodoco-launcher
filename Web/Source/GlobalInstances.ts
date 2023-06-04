import { writable } from "svelte/store";
import i18next from "i18next";
// Generated types
import { ILauncher } from "./Generated/Dodoco/Launcher/ILauncher";
import { IGame } from "./Generated/Dodoco/Game/IGame";

export const LauncherInstance = writable({} as ILauncher);
export const GameInstance = writable({} as IGame);
export const i18nInstance = writable(i18next);