
<script lang="ts">

    import { Nullable } from "..";

    export let legend: Nullable<string> = null;
    export let value: Nullable<string> = null;

</script>

<fieldset>
    {#if legend}
        <legend>{ legend }</legend>
    {/if}
    <input bind:value={value} type="text" spellcheck="false"/>
</fieldset>

<style lang="postcss">

    fieldset {

        @apply w-full overflow-x-hidden flex flex-col items-start transition-all ease-in-out rounded-md border-2 border-gray-200

    }

    fieldset:focus-within {

        @apply text-golden

    }

    fieldset input[type="text"] {

        @apply w-full py-2 px-4 text-sm caret-zinc-700 outline-none focus:border-b-golden focus:text-golden

    }

    fieldset legend {

        @apply text-left text-xs hy-impact-font focus:text-golden ms-2 px-2

    }

</style>