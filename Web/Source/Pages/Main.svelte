<script lang="ts">
    
    import { _GameState, _MainViewData, _LauncherDependency, _WinePackageManagerState, i18nInstance } from "@Dodoco/Global";
    import { Button, ButtonGroup, ConfirmPopup, ConfirmPopupControl, ModalControl, Popup, ProgressBar } from "@Dodoco/Components";
    import { DataUnitFormatter } from "@Dodoco/Util";
    import Settings from "./Settings.svelte";
    import { onMount } from "svelte";
    import Icon from "@iconify/svelte";

    // Generated types
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { LauncherDependency } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherDependency";
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Core/Launcher/Settings/LauncherSettings";
    import { Release } from "@Dodoco/Generated/Dodoco/Core/Network/Api/Github/Repos/Release/Release";
    import { WinePackageManagerState } from "@Dodoco/Generated/Dodoco/Core/Wine/WinePackageManagerState";
    import { WineController } from "@Dodoco/Generated/Dodoco/Application/Control/WineController";
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Application/Control/LauncherController";
    import { GameController } from "@Dodoco/Generated/Dodoco/Application/Control/GameController";
    import { CommonErrorData } from "..";

    let mainWrapper: HTMLDivElement;
    let settingsModal: ModalControl;
    let errorModalControl: ConfirmPopupControl;
    let confirmGameDownload: ConfirmPopupControl;
    let errorData: CommonErrorData = { code: 0 };
    let busyOperationProgress: number = 0;
    let latestWineRelease: Promise<Release>;

    $: LauncherIsWaiting = (): boolean => {

        return ($_LauncherDependency ?? LauncherDependency.NONE) != LauncherDependency.NONE;

    }

    $: LauncherIsBusy = (): boolean => {

        return ($_GameState ?? GameState.READY) != GameState.READY
        || ($_WinePackageManagerState ?? WinePackageManagerState.READY) != WinePackageManagerState.READY;

    }

    onMount(async () => {

        // Set launcher's background image

        mainWrapper.style.backgroundImage = "url('data:image/png;base64," + await LauncherController.GetControllerInstance().GetLauncherBackgroundImage() + "')";

        latestWineRelease = WineController.GetControllerInstance().GetLatestRelease();

        setInterval(async () => {

            if (LauncherIsBusy()) {

                if ($_GameState == GameState.CHECKING_INTEGRITY) {

                    busyOperationProgress = $_MainViewData._ProgressReport?.CompletionPercentage ?? 0;

                }

            }

        }, 500);

        confirmGameDownload.OnChoose.Add(async (s, e) => {

            if (e) {

                try {

                    await GameController.GetControllerInstance().Download();

                } catch (err: any) {

                    if (err.data) {

                        errorData = err.data as CommonErrorData;
                        errorModalControl.Open();
                        return;

                    }

                }

            }

        });

    });

    async function MainButtonClick() {

        try {

            if ($_LauncherDependency == LauncherDependency.WINE_DOWNLOAD) {

                await WineController.GetControllerInstance().InstallLatestRelease();
                let settings: LauncherSettings = await LauncherController.GetControllerInstance().GetLauncherSettings();
                settings.Wine.SelectedRelease = (await latestWineRelease).tag_name;
                await LauncherController.GetControllerInstance().SetLauncherSettings(settings);

            } else if ($_LauncherDependency == LauncherDependency.GAME_DOWNLOAD) {

                confirmGameDownload.Open();

            } else if ($_LauncherDependency == LauncherDependency.GAME_UPDATE) {

                GameController.GetControllerInstance().Update();

            }

        } catch (err: any) {

            if (err.data) {

                errorData = err.data as CommonErrorData;
                errorModalControl.Open();
                return;

            }

        }

    }

    $: GetProgress = (): number => {

        return Math.floor($_MainViewData._ProgressReport?.CompletionPercentage ?? 0);

    }

    $: GetBytesPerSecond = (): string => {

        return DataUnitFormatter.Format($_MainViewData._ProgressReport?.BytesPerSecond ?? 0) + "/s";

    }

    $: GetEta = (): string => {

        if ($_MainViewData._ProgressReport) {

            return ($_MainViewData._ProgressReport.EstimatedRemainingTime as string).split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":");

        } else {

            return "";

        }

    }

</script>
<div bind:this={mainWrapper} class="w-full h-full bg-center bg-cover bg-no-repeat">
    <div class="w-full h-full bg-gradient-to-t from-black/80 to-black/0">
        <div class="w-full h-full w-full flex flex-col items-center p-4">
            <div class="w-full h-full flex flex-col">
                <div class="w-full h-full flex flex-col items-center justify-end pb-12 gap-y-3">
                    <small class="text-white animate-pulse text-center">
                        {#if $_LauncherDependency != LauncherDependency.NONE }
                            {#if $_LauncherDependency == LauncherDependency.GAME_DOWNLOAD }
                                {#if $_GameState == GameState.DOWNLOADING }
                                    { $i18nInstance.t("main.message.game_state.downloading", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetBytesPerSecond() }) }
                                {:else if $_GameState == GameState.RECOVERING_DOWNLOADED_SEGMENTS }
                                    { $i18nInstance.t("main.message.game_state.recovering_downloaded_segments", { percentage: GetProgress() }) }
                                {:else if $_GameState == GameState.EXTRACTING_DOWNLOADED_SEGMENTS }
                                    { $i18nInstance.t("main.message.game_state.extracting_downloaded_segments", { percentage: GetProgress(), remaining_time: GetEta(), decompression_rate: GetBytesPerSecond() }) }
                                {:else}
                                    { $i18nInstance.t("main.message.dependency.game_download") }
                                {/if}
                            {:else if $_LauncherDependency == LauncherDependency.GAME_UPDATE }
                                {#if $_GameState == GameState.DOWNLOADING_UPDATE }
                                    { $i18nInstance.t("main.message.game_state.downloading_update", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetBytesPerSecond() }) }
                                {:else if $_GameState == GameState.EXTRACTING_UPDATE }
                                    { $i18nInstance.t("main.message.game_state.extracting_update") }
                                {:else if $_GameState == GameState.PATCHING_FILES }
                                    { $i18nInstance.t("main.message.game_state.patching_files", { percentage: GetProgress() }) }
                                {:else if $_GameState == GameState.REMOVING_DEPRECATED_FILES }
                                    { $i18nInstance.t("main.message.game_state.removing_deprecated_files", { percentage: GetProgress() }) }
                                {:else}
                                    { $i18nInstance.t("main.message.dependency.game_update") }
                                {/if}
                            {:else if $_LauncherDependency == LauncherDependency.WINE_CONFIGURATION }
                                { $i18nInstance.t("main.message.dependency.wine_configuration") }
                            {:else if $_LauncherDependency == LauncherDependency.WINE_DOWNLOAD }
                                {#if $_WinePackageManagerState == WinePackageManagerState.DOWNLOADING_PACKAGE }
                                    { $i18nInstance.t("main.message.wine_package_manager_state.downloading_package", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetBytesPerSecond() }) }
                                {:else if $_WinePackageManagerState == WinePackageManagerState.DECOMPRESSING_PACKAGE }
                                    { $i18nInstance.t("main.message.wine_package_manager_state.decompressing_package") }
                                {:else if $_WinePackageManagerState == WinePackageManagerState.CHECKING_PACKAGE_CHECKSUM }
                                    { $i18nInstance.t("main.message.wine_package_manager_state.checking_package_checksum") }
                                {:else}
                                    { $i18nInstance.t("main.message.dependency.wine_download") }
                                {/if}
                            {:else}
                                { $i18nInstance.t("main.message.dependency.other") }
                            {/if}
                        {:else}
                            {#if $_GameState == GameState.CHECKING_INTEGRITY }
                                { $i18nInstance.t("main.message.game_state.checking_integrity", { percentage: GetProgress(), remaining_time: GetEta() }) }
                            {:else if $_GameState == GameState.REPAIRING_FILES }
                                { $i18nInstance.t("main.message.game_state.repairing_files", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetBytesPerSecond() }) }
                            {/if}
                        {/if}
                        {#if LauncherIsBusy() && $_MainViewData._ProgressReport?.Message }
                            <br>{$_MainViewData._ProgressReport.Message}
                        {/if}
                    </small>
                    {#if LauncherIsBusy() && GetProgress() != 0 }
                        <ProgressBar value={GetProgress()} width="1/4"/>
                    {/if}
                    <ButtonGroup>
                        <Button on:click={() => settingsModal.Open() } disabled={LauncherIsBusy()}><Icon icon="material-symbols:settings"/>&nbsp;{$i18nInstance.t("settings.title")}</Button>
                        <Button focused disabled={LauncherIsBusy()} on:click={() => MainButtonClick()}>
                            {#if $_LauncherDependency == LauncherDependency.WINE_DOWNLOAD }
                                {#await latestWineRelease then result}
                                    <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.download_wine", { version: result.tag_name } ) }
                                {/await}
                            {:else if $_LauncherDependency == LauncherDependency.GAME_DOWNLOAD }
                                <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.download_game") }
                            {:else if $_LauncherDependency == LauncherDependency.GAME_UPDATE }
                                <Icon icon="material-symbols:deployed-code-update-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.update_game") }
                            {:else if $_LauncherDependency == LauncherDependency.NONE }
                                <Icon icon="material-symbols:play-arrow-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.start_game") }
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
    <Popup bind:Root={errorModalControl} title={`${$i18nInstance.t("component.popup.error_title")} ${errorData.code}`} callback={() => {}}>
        <p>
            {#if errorData.type}
                <strong>Type:</strong><br>
                {errorData.type}<br>
            {/if}
            {#if errorData.message}
                <strong>Message:</strong><br>
                {errorData.message}<br>
            {/if}
            {#if errorData.stack}
                <strong>Stack:</strong><br>
                {errorData.stack}<br>
            {/if}
            {#if errorData.inner}
                <strong>Inner exception:</strong>
                {JSON.stringify(errorData.inner)}
            {/if}
        </p>
    </Popup>
    <ConfirmPopup bind:Root={confirmGameDownload} callback={(e) => null}>
        {#await LauncherController.GetControllerInstance().GetLauncherSettings() then result }
            <p>{ $i18nInstance.t("main.confirm.game_download.text", { directory: result.Game.InstallationDirectory, menu_path: `${$i18nInstance.t("settings.title")} > ${$i18nInstance.t("settings.menu.game")}` }) }</p>
        {/await}
    </ConfirmPopup>
    <Settings bind:Root={settingsModal}/>
</div>
<style>
</style>