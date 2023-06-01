<script lang="ts">
    
    import { blur } from "svelte/transition";

    export let show: boolean = true;
    export let value: number = 0;
    export let width: string = "full";
    export let height: string = "2";

</script>
{#if show}
    <div transition:blur={{ duration: 250 }} class="w-{width} h-{height} bg-black/20 flex flex-row justify-start items-center rounded-full">
        <div class="h-full bg-yellow-300 rounded-full" style="width: {Math.min(value, 100)}%;"></div>
    </div>
{/if}