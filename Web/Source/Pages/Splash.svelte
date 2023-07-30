<script lang="ts">

    import { _SplashViewData } from "@Dodoco/Global";
    import { LoadingDots } from "@Dodoco/Components";
    // Generated types
    import { LogEntry } from "@Dodoco/Generated/Dodoco/Core/Util/Log/LogEntry";
    import { LogType } from "@Dodoco/Generated/Dodoco/Core/Util/Log/LogType";

    function GetLogTextColor(entry: LogEntry): string {

        switch (entry.type) {

            case LogType.LOG:
                return "text-slate-400/70";
            case LogType.WARNING:
                return "text-amber-400";
            case LogType.ERROR:
                return "text-red-500";
            case LogType.DEBUG:
                return "text-sky-400";

        }

        return "";

    }

</script>
<!-- svelte-ignore a11y-missing-attribute -->
<div class="w-screen h-screen flex flex-col items-center justify-center p-1">
    <img src="/Image/klee_by_kkomdastro.webp" class="object-contain h-1/2 drop-shadow-md"/>
    <div class="flex flex-col items-center justify-center gap-y-2">
        <h1 class="text-red-600 text-xl hy-impact-font font-medium drop-shadow-md uppercase">Loading<LoadingDots/></h1>
        {#if $_SplashViewData.LogEntry }
            <small class="{GetLogTextColor($_SplashViewData.LogEntry)} text-center text-xs hy-impact-font drop-shadow-md">{$_SplashViewData.LogEntry.message}</small>
        {/if}
    </div>
</div>