import type { LogType } from "./LogType";

export interface LogEntry {

    type: LogType;
    prependMessage: string;
    message: string;

}