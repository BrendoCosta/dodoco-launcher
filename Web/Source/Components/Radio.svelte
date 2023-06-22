<script lang="ts">

    import { RadioGroup } from "./RadioGroup";
    
    import { onMount } from "svelte";
    import { forwardEventsBuilder } from "@smui/common/internal";
    import { get_current_component } from "svelte/internal";
    const forwardEvents = forwardEventsBuilder(get_current_component());

    export let show: boolean = true;
    export let selected: boolean = false;
    export let group: RadioGroup<any>;
    export let value: any;

    onMount(() => {

        if (selected) MarkAsSelected()

    });

    let MarkAsSelected = () => {

        group.Selected = value;
        group = group;

    }

</script>
{#if show}
    <div on:click={MarkAsSelected} use:forwardEvents class="input radio">
        {#if group.Selected == value }
            <div class="check"></div>
        {/if}
    </div>
{/if}