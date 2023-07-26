<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { i18nInstance } from "@Dodoco/Global";
    import { ConfirmPopupControl, Modal, ScoopedFrame } from "./";
    import { Nullable } from "..";

    export let title: Nullable<string> = null;
    export let confirm: Nullable<string> = null;
    export let callback: (result: boolean) => void;
    export let Root: ConfirmPopupControl = new ConfirmPopupControl(callback);

</script>
<Modal bind:Root={Root}>
    <ScoopedFrame width="[50%]" height="auto">
        <div class="w-full h-full flex flex-col items-start m-2 gap-y-2">
            <div class="w-full h-4/5 flex flex-col justify-center gap-y-2">
                <h1 class="text-2xl font-medium text-gray-800 drop-shadow-md">{ title ?? $i18nInstance.t("component.popup.default_title") }</h1>
                <hr class="w-full border-[0.1rem] border-gray-300"/>
                <div class="w-full max-h-[6rem] overflow-y-scroll text-sm text-gray-800">
                    <slot/>
                </div>
            </div>
            <div class="w-full h-1/5 inline-flex justify-center gap-x-6">
                <button class="input w-full" on:click={() => Root.Choose(true) }>{ confirm ?? $i18nInstance.t("component.popup.button_confirm") }</button>
            </div>
        </div>
    </ScoopedFrame>
</Modal>
