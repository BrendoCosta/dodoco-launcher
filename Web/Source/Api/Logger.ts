import type { LogEntry } from "@Dodoco/Generated/Dodoco.Util.Log";

export class Logger {

    public static async GetFullLogJson() {

        return new Promise<LogEntry[]>((resolve, reject) => {

            fetch("http://localhost:4000/Dodoco/LoggerController/GetFullLogJson").then(async (response: Response) => {

                if (response.ok) return ((await response.json()) as LogEntry[]);
                else throw response;

            }).then((json: LogEntry[]) => {

                resolve(json)

            }).catch((error: any) => {

                reject(error);

            });

        });
        
    }

}