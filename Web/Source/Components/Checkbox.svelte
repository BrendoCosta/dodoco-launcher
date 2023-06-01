<script lang="ts">

    import Icon from "@iconify/svelte";

    export let show: boolean = true;
    export let checked: boolean = false;

</script>
{#if show}
    <div on:click={() => (checked = !checked)} class="input w-6 h-6 flex flex-row justify-center items-center">
        {#if checked}
            <Icon icon="mingcute:check-fill" />
        {/if}
    </div>
{/if}