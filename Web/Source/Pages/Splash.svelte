<script lang="ts">
    import { onMount } from "svelte";
    import { Nullable } from "@Dodoco/index";
    import { RpcClient } from "@Dodoco/Backend";
    import { LoadingDots } from "@Dodoco/Components";
    // Generated types
    import { LogEntry } from "@Dodoco/Generated/Dodoco/Util/Log/LogEntry";
    import { LogType } from "@Dodoco/Generated/Dodoco/Util/Log/LogType";

    let title: string = "Loading";
    let logMessage: LogEntry = { type: LogType.LOG, message: "", prependMessage: "" };
    let logMessageTextColor: string = "";

    onMount(async () => {

        setInterval(async () => {

            let fetchLogMessage: Nullable<LogEntry> = await RpcClient.GetInstance().Call("Dodoco.Util.Log.Logger.GetLastLogEntry");

            if (fetchLogMessage != null && fetchLogMessage.type != LogType.DEBUG) {

                logMessage = fetchLogMessage;

                switch (fetchLogMessage.type) {

                    case LogType.LOG:
                        logMessageTextColor = "text-slate-400/70";
                        break;
                    case LogType.WARNING:
                        logMessageTextColor = "text-amber-400";
                        break;
                    case LogType.ERROR:
                        logMessageTextColor = "text-red-500";
                        break;
                    /*
                    case LogType.DEBUG:
                        logMessageTextColor = "text-sky-400";
                        break;
                    */

                }

            }

        }, 100);

	});

</script>
<!-- svelte-ignore a11y-missing-attribute -->
<div class="w-screen h-screen flex flex-col items-center justify-center">
    <img src="/Image/klee_by_kkomdastro.webp" class="object-contain h-1/2 drop-shadow-md"/>
    <div class="flex flex-col items-center justify-center gap-y-2">
        <h1 class="text-red-600 text-xl font-['HYImpact'] font-medium drop-shadow-md uppercase">{title}<LoadingDots/></h1>
        <small class="{logMessageTextColor} text-center text-xs font-['HYImpact'] drop-shadow-md">{logMessage.message}</small>
    </div>
</div>