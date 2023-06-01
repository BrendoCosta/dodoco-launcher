<script lang="ts">

    import { createDialog } from "svelte-headlessui";
	import { blur } from "svelte/transition";

    const dialog = createDialog();

    export let show: boolean = false;

    export function Open(): void {

        dialog.open();
        show = true;

    }
    
    export function IsOpen(): boolean {

        return show;

    }

    export function Close(): void {

        dialog.close();
        show = false;

    }

</script>
{#if show}
    <div transition:blur={{ duration: 250 }} class="fixed inset-0 overflow-y-auto flex min-h-full items-center justify-center p-4 text-center">
        <div class="fixed inset-0 bg-black bg-opacity-50" on:click={() => Close()} on:keydown={() => Close()}/>
        <slot/>
    </div>
{/if}
