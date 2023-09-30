<script lang="ts">

    import { forwardEventsBuilder } from "@smui/common/internal";
    import { get_current_component } from "svelte/internal";
    const forwardEvents = forwardEventsBuilder(get_current_component());

    export let selected: any;
    export let value: any;

</script>
<input class="opacity-0 w-0 h-0" bind:group={selected} type="radio" value={value} />
<div data-role="input radio" on:click={() => (selected = value)} use:forwardEvents>
    {#if selected == value}
        <div data-role="check"/>
    {/if}
</div>

<style lang="postcss">

    .dodoco.input.radio {

        @apply rounded-full w-6 h-6 border-[2px] border-gray-200 flex flex-row justify-center items-center p-1

    }

    .dodoco.input.radio .check {

        @apply rounded-[50%] bg-zinc-700 w-full h-full

    }

</style>