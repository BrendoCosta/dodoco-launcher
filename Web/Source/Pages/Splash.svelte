<script lang="ts">

    import { onMount } from "svelte";
    import { DodocoSSE } from "../Service";
    import type { LogEntry } from "../Common/Util";

    let title: string = "LOADING...";
    let logMessage: string = "";

    onMount(async () => {

		DodocoSSE.addEventListener("Dodoco.Util.Log.Logger.GetLastLogJson", (event: MessageEvent<string>) => {

            let data: LogEntry = JSON.parse(event.data);
            logMessage = data.message;

            if (title.slice(-3) == "...") title = title.slice(0, -3);
            else title += ".";

        });

	});

</script>
<!-- svelte-ignore a11y-missing-attribute -->
<img src="/Image/klee_by_kkomdastro.webp" class="splash-image"/>
<h3 class="splash-title">{title}</h3>
<small class="splash-text">{logMessage}</small>