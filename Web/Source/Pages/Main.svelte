<script lang="ts">
    
    import { _MainViewData } from "@Dodoco/Global";
    import { Button, ButtonGroup, ModalControl, ProgressBar } from "@Dodoco/Components";
    import Settings from "./Settings.svelte";

    // Generated types
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { MainController } from "@Dodoco/Generated/Dodoco/Application/Control/MainController";

    import { onMount } from "svelte";
    import Icon from "@iconify/svelte";
    
    let mainWrapper: HTMLDivElement;
    let settingsModal: ModalControl;
    let busyOperationProgress: number = 0;

    $: IsBusy = (): boolean => {
        return $_MainViewData.Game?.State == GameState.CHECKING_INTEGRITY
            || $_MainViewData.Game?.State == GameState.DOWNLOADING
            || $_MainViewData.Game?.State == GameState.RUNNING
            || $_MainViewData.Game?.State == GameState.REPAIRING_FILES
            || $_MainViewData.Game?.State == GameState.UPDATING;
    }

    onMount(async () => {

        mainWrapper.style.backgroundImage = "url('data:image/png;base64," + await MainController.GetControllerInstance().GetBackroundImage() + "')";

        setInterval(async () => {

            if (IsBusy()) {

                if ($_MainViewData.Game?.State == GameState.CHECKING_INTEGRITY) {

                    busyOperationProgress = $_MainViewData.ApplicationProgressReport.completionPercentage;

                }

            }

        }, 500);

    });

    let selectedTableHelper = (a: any) => a.selected ? "my-custom-class active" : "my-custom-class";

</script>
<div bind:this={mainWrapper} class="w-full h-full bg-center bg-cover bg-no-repeat">
    <div class="w-full h-full bg-gradient-to-t from-black/80 to-black/0">
        <div class="w-full h-full w-full flex flex-col items-center p-4">
            <div class="w-full h-full flex flex-col">
                <div class="w-full h-full flex flex-col items-center justify-end pb-12 gap-y-3">
                    {#if IsBusy() }
                        <small class="text-white animate-pulse">
                            {#if $_MainViewData.Game?.State == GameState.CHECKING_INTEGRITY }
                                Checking game's files integrity ({Math.floor($_MainViewData.ApplicationProgressReport.completionPercentage)}% / ETA: {$_MainViewData.ApplicationProgressReport.estimatedRemainingTime.toString().split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":")});
                            {:else if $_MainViewData.Game?.State == GameState.REPAIRING_FILES }
                                Repairing game's files
                            {:else}
                                Working...
                            {/if}
                        </small>
                        <ProgressBar value={busyOperationProgress} width="1/4"/>
                    {/if}
                    <ButtonGroup>
                        <Button on:click={() => settingsModal.Open() } disabled={IsBusy()}><Icon icon="material-symbols:settings"/>&nbsp;Settings</Button>
                        <Button focused disabled={IsBusy()}>
                            {#if $_MainViewData.Game?.State == GameState.WAITING_FOR_DOWNLOAD }
                                <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;Download Game
                            {:else if $_MainViewData.Game?.State == GameState.WAITING_FOR_UPDATE }
                                <Icon icon="material-symbols:deployed-code-update-rounded" />&nbsp;Update Game
                            {:else if $_MainViewData.Game?.State == GameState.READY }
                                <Icon icon="material-symbols:play-arrow-rounded" />&nbsp;Start Game
                            {:else}
                                <Icon icon="material-symbols:autorenew-outline-rounded" class="animate-spin"/>&nbsp;Unready
                            {/if}
                        </Button>
                        <Button><Icon icon="material-symbols:info"/>&nbsp;Changelog</Button>
                    </ButtonGroup>
                </div>
            </div>
        </div>
    </div>
    <Settings bind:Root={settingsModal}/>
</div>
<style>
</style>