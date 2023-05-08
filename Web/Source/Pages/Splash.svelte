<script lang="ts">

    import { onMount } from "svelte";
    import { DodocoSSE } from "../Service";
    import type { LogEntry } from "../Common/Util";

    let title: string = "LOADING...";
    let logMessage: LogEntry = {
        type: 0,
        message: "",
        prependMessage: "",
    };

    onMount(async () => {

		DodocoSSE.addEventListener("Dodoco.Util.Log.Logger.Log", (event: MessageEvent<string>) => logMessage = JSON.parse(event.data));
        DodocoSSE.addEventListener("Dodoco.Util.Log.Logger.Warning", (event: MessageEvent<string>) => logMessage = JSON.parse(event.data));

        setInterval(() => {

            if (title.slice(-3) == "...") title = title.slice(0, -3);
            else title += ".";

        }, 250)

	});

</script>
<!-- svelte-ignore a11y-missing-attribute -->
<img src="/Image/klee_by_kkomdastro.webp" class="splash-image"/>
<h3 class="splash-title">{title}</h3>
<small class="splash-text {logMessage.type == 3 ? "warning" : "" }">{logMessage.message}</small>