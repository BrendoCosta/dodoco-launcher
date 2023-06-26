<script lang="ts">
    
    import { _MainViewData, i18nInstance } from "@Dodoco/Global";
    import { Button, ButtonGroup, ConfirmPopupControl, ModalControl, Popup, ProgressBar } from "@Dodoco/Components";
    import Settings from "./Settings.svelte";

    // Generated types
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { MainController } from "@Dodoco/Generated/Dodoco/Application/Control/MainController";

    import { onMount } from "svelte";
    import Icon from "@iconify/svelte";
    
    let mainWrapper: HTMLDivElement;
    let settingsModal: ModalControl;
    let errorModalControl: ConfirmPopupControl;
    let errorMessage: string;
    let busyOperationProgress: number = 0;

    $: IsBusy = (): boolean => {
        return $_MainViewData.Game?.State != GameState.READY
            && $_MainViewData.Game?.State != GameState.NOT_INSTALLED
            && $_MainViewData.Game?.State != GameState.WAITING_FOR_DOWNLOAD
            && $_MainViewData.Game?.State != GameState.WAITING_FOR_UPDATE;
    }

    onMount(async () => {

        mainWrapper.style.backgroundImage = "url('data:image/png;base64," + await MainController.GetControllerInstance().GetBackroundImage() + "')";

        setInterval(async () => {

            if (IsBusy()) {

                if ($_MainViewData.Game?.State == GameState.CHECKING_INTEGRITY) {

                    busyOperationProgress = $_MainViewData.ProgressReport.CompletionPercentage;

                }

            }

        }, 500);

    });

    async function MainButtonClick() {

        if ($_MainViewData.Game?.State == GameState.WAITING_FOR_UPDATE) {

            MainController.GetControllerInstance().UpdateGame().catch(e => {

                errorMessage = (e as Error).message;
                errorModalControl.Open();

            });

        }

    }

    function FormatBytes(bytes: number) {
        
        if (bytes === 0) return "0.00 B"
        
        let e = Math.floor(Math.log(bytes) / Math.log(1000));
        return (bytes / Math.pow(1000, e)).toFixed(2) + " " + " KMGTP".charAt(e) + "B";

    }

    $: GetProgress = (): number => {

        return Math.floor($_MainViewData.ProgressReport.CompletionPercentage);

    }

    $: GetBytesPerSecond = (): string => {

        return FormatBytes($_MainViewData.ProgressReport.BytesPerSecond);

    }

    $: GetEta = (): string => {

        return ($_MainViewData.ProgressReport.EstimatedRemainingTime as string).split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":");

    }

</script>
<div bind:this={mainWrapper} class="w-full h-full bg-center bg-cover bg-no-repeat">
    <div class="w-full h-full bg-gradient-to-t from-black/80 to-black/0">
        <div class="w-full h-full w-full flex flex-col items-center p-4">
            <div class="w-full h-full flex flex-col">
                <div class="w-full h-full flex flex-col items-center justify-end pb-12 gap-y-3">
                    {#if IsBusy() }
                        <small class="text-white animate-pulse text-center">
                            {#if $_MainViewData.Game?.State == GameState.CHECKING_INTEGRITY }
                                Checking game's files integrity ({GetProgress()}% / ETA: {GetEta()})
                            {:else if $_MainViewData.Game?.State == GameState.REPAIRING_FILES }
                                Repairing game's files ({GetProgress()}% / ETA: {GetEta()} / Download rate: {GetBytesPerSecond()})
                            {:else if $_MainViewData.Game?.State == GameState.DOWNLOADING_UPDATE }
                                Downloading game's update ({GetProgress()}% / ETA: {GetEta()} / Download rate: {GetBytesPerSecond()})
                            {:else if $_MainViewData.Game?.State == GameState.EXTRACTING_UPDATE }
                                Decompressing the game's update
                            {:else if $_MainViewData.Game?.State == GameState.PATCHING_FILES }
                                Patching the game's files ({GetProgress()}%)
                            {:else if $_MainViewData.Game?.State == GameState.REMOVING_DEPRECATED_FILES }
                                Removing deprecated files ({GetProgress()}%)
                            {:else}
                                Working
                            {/if}
                            {#if $_MainViewData.ProgressReport.Message }
                                <br>{$_MainViewData.ProgressReport.Message}
                            {/if}
                        </small>
                        <ProgressBar value={GetProgress()} width="1/4"/>
                    {/if}
                    <ButtonGroup>
                        <Button on:click={() => settingsModal.Open() } disabled={IsBusy()}><Icon icon="material-symbols:settings"/>&nbsp;Settings</Button>
                        <Button focused disabled={IsBusy()} on:click={() => MainButtonClick()}>
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
    <Popup bind:Root={errorModalControl} title={$i18nInstance.t("component.popup.error_title")} callback={() => {}}>
        <p>{ errorMessage }</p>
    </Popup>
</div>
<style>
</style>