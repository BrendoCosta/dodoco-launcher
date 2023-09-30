<script lang="ts">

    import { onMount } from "svelte";

    let text: string = "...";

    onMount(() => {

        setInterval(async () => {

            if (text.endsWith("...")) {
                
                text = text.slice(0, -3);

            } else {

                text += ".";

            }

        }, 500);

    });

</script>
<span>{text}</span>