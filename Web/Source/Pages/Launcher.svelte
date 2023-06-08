<script lang="ts">

    import { Nullable } from "@Dodoco/index";
    import { GameInstance, i18nInstance } from "@Dodoco/GlobalInstances";
    import { Button, ButtonGroup, ModalControl, ProgressBar } from "@Dodoco/Components";
    import Settings from "./Settings.svelte";
    
    import { onMount } from "svelte";
    import Icon from "@iconify/svelte";
    
    // Generated types
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Network/Controller/LauncherController";
    import { GameState } from "@Dodoco/Generated/Dodoco/Game/GameState";
    import { ApplicationProgressReport } from "@Dodoco/Generated/Dodoco/Application/ApplicationProgressReport";

    $: game = $GameInstance;
    $: _i18n = $i18nInstance;
    let settingsModal: ModalControl;
    let checkIntegrityProgressReport: Nullable<ApplicationProgressReport> = null;
    let busyOperationProgress: number = 0;

    $: IsBusy = (): boolean => {
        return game.State == GameState.CHECKING_INTEGRITY
            || game.State == GameState.DOWNLOADING
            || game.State == GameState.RUNNING
            || game.State == GameState.REPAIRING_FILES
            || game.State == GameState.UPDATING;
    }

    onMount(() => {

        setInterval(async () => {

            if (IsBusy()) {

                if (game.State == GameState.CHECKING_INTEGRITY) {

                    checkIntegrityProgressReport = await LauncherController.GetInstance().GetLastGameCheckIntegrityProgressReport();
                    busyOperationProgress = checkIntegrityProgressReport.completionPercentage;

                }

            }

        }, 500);

    });

    let selectedTableHelper = (a: any) => a.selected ? "my-custom-class active" : "my-custom-class";

</script>
<div class="w-full h-full bg-[url('/.cache/background.png')] bg-center bg-cover bg-no-repeat">
    <div class="w-full h-full bg-gradient-to-t from-black/80 to-black/0">
        <div class="w-full h-full w-full flex flex-col items-center p-4">
            <div class="w-full h-full flex flex-col">
                <div class="w-full h-full flex flex-col items-center justify-end pb-12 gap-y-3">
                    {#if IsBusy() }
                        <small class="text-white animate-pulse">
                            {#if game.State == GameState.CHECKING_INTEGRITY }
                                {#if checkIntegrityProgressReport != null }
                                    Checking game's files integrity ({Math.floor(checkIntegrityProgressReport.completionPercentage)}% / ETA: {checkIntegrityProgressReport.estimatedRemainingTime.toString().split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":")})
                                {/if}
                            {:else if game.State == GameState.REPAIRING_FILES }
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
                            {#if game.State == GameState.WAITING_FOR_DOWNLOAD }
                                <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;Download Game
                            {:else if game.State == GameState.WAITING_FOR_UPDATE }
                                <Icon icon="material-symbols:deployed-code-update-rounded" />&nbsp;Update Game
                            {:else if game.State == GameState.READY }
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