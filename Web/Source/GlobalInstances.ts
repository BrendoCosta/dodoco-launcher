import { writable } from "svelte/store";
// Generated types
import { ILauncher } from "./Generated/Dodoco/Launcher/ILauncher";
import { IGame } from "./Generated/Dodoco/Game/IGame";

export const LauncherInstance = writable({} as ILauncher);
export const GameInstance = writable({} as IGame);