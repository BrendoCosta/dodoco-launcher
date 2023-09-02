<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { i18nInstance } from "@Dodoco/Global";
    import { ConfirmPopupControl, Modal, ScoopedFrame } from "./";
    import { onMount } from "svelte";

    export let title: string = "Confirm";
    export let yes: string = "Yes";
    export let no: string = "No";
    export let open: boolean = false;
    export let callback: (result: boolean) => void;
    export let Root: ConfirmPopupControl = new ConfirmPopupControl(callback);

    onMount(() => {

        if (open)
            Root.Open();

    });

</script>
<Modal bind:Root={Root}>
    <ScoopedFrame width="[35%]" height="auto">
        <div class="w-full h-full flex flex-col items-start m-2 gap-y-2">
            <div class="w-full h-4/5 flex flex-col justify-center gap-y-2">
                <h1 class="text-2xl hy-impact-font font-medium text-gray-800 drop-shadow-md">{ $i18nInstance.t("component.confirm.default_title") }</h1>
                <hr class="w-full border-[0.1rem] border-gray-300"/>
                <div class="w-full max-h-[6rem] overflow-y-scroll text-gray-800">
                    <slot/>
                </div>
            </div>
            <div class="w-full h-1/5 inline-flex justify-center gap-x-6">
                <button data-role="button" class="w-full" on:click={() => Root.Choose(false) }>{ $i18nInstance.t("component.confirm.button_false") }</button>
                <button data-role="button" data-highlight class="w-full" on:click={() => Root.Choose(true) }>{ $i18nInstance.t("component.confirm.button_true") }</button>
            </div>
        </div>
    </ScoopedFrame>
</Modal>
