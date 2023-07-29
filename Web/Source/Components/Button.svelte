<script lang="ts">
    
    import { onMount } from "svelte";

    import { forwardEventsBuilder } from "@smui/common/internal";
    import { get_current_component } from "svelte/internal";
    const forwardEvents = forwardEventsBuilder(get_current_component());

    export let _class: string = "";
    export let element: HTMLButtonElement | undefined = undefined;
    export let additionalClass: string = "";
    export let active: boolean = false;
    export let disabled: boolean = false;
    export let focused: boolean = false;
    export { _class as class };

    onMount(() => {

        if (focused)
            element?.focus();

    });

    function OnClick() {

        element?.classList.add("button-click");
        setTimeout(() => { element?.classList.remove("button-click") }, 500);

    };

</script>
<button disabled={disabled} bind:this={element} on:click={() => OnClick()} use:forwardEvents class="
    {_class != "" ? _class : `
        h-full transition-all ease-in-out bg-gradient-to-t from-yellow-500 to-yellow-400 text-yellow-700 font-normal p-4 flex flex-row items-center
        hover:to-yellow-300
        [&.active]:bg-yellow-300
        focus:bg-yellow-300
    ` }
    { active ? "active" : "" }
    { disabled ? "opacity-50" : "" }
    { additionalClass }
">
    <slot/>
</button>
<style>

    @keyframes button-click-animation {
        0%  {  }
        50%  {  }
        100% {  }
    }

    .button-click {

        animation-name: button-click-animation;
        animation-duration: 500ms;

    }
    
</style>