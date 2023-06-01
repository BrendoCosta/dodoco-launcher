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
        transition-all ease-in-out bg-transparent text-white/50 font-normal border-white/50 p-4 flex flex-row items-center
        hover:bg-transparent hover:border-yellow-300 hover:text-yellow-300 hover:ring-0 hover:outline-0 hover:shadow-[inset_0_0_0_0.2rem_black] hover:shadow-yellow-300
        [&.active]:bg-yellow-300 [&.active]:text-yellow-700 [&.active]:ring-0 [&.active]:outline-0
        focus:bg-yellow-300 focus:text-yellow-700 focus:ring-0 focus:outline-0
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