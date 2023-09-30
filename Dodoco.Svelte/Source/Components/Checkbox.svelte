<script lang="ts">

    import Icon from "@iconify/svelte";

    export let show: boolean = true;
    export let checked: boolean = false;

</script>
{#if show}
    <div data-role="input checkbox" on:click={() => (checked = !checked)} data-checked={checked ? "checked" : undefined}>
        {#if checked}
            <Icon icon="mingcute:check-fill" />
        {/if}
    </div>
{/if}