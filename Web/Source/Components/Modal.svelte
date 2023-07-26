<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { ModalControl } from "./ModalControl";
    import { blur } from "svelte/transition";

    export let Root: ModalControl = new ModalControl();
    export let closable: boolean = true;
    $: Properties = Root.Properties

</script>
{#if $Properties.IsOpen }
    <div class="relative z-10">
        <div transition:blur={{ duration: 250 }} class="fixed inset-0 overflow-y-auto flex min-h-full items-center justify-center p-4 text-center">
            <div class="fixed inset-0 bg-black bg-opacity-50" on:click={() => closable ? Root.Close() : null} on:keydown={() => closable ? Root.Close() : null}/>
            <slot/>
        </div>
    </div>
{/if}
