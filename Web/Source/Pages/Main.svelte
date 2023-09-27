<script lang="ts">
    
    import { CommonErrorData, ErrorHandler } from "@Dodoco/index";
    import { _AppError, _GameState, _GameIntegrityCheckState, _GameUpdateState, _MainViewData, _LauncherDependency, _UiStatesHelpers, _WinePackageManagerState, i18nInstance, _GameDownloadState } from "@Dodoco/Global";
    import { Button, ButtonGroup, ConfirmPopup, ConfirmPopupControl, ModalControl, Popup, ProgressBar } from "@Dodoco/Components";
    import { DataUnitFormatter } from "@Dodoco/Util";
    import Settings from "./Settings.svelte";
    import { onMount } from "svelte";
    import Icon from "@iconify/svelte";

    // Generated types
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { GameDownloadState } from "@Dodoco/Generated/Dodoco/Core/Game/GameDownloadState";
    import { GameIntegrityCheckState } from "@Dodoco/Generated/Dodoco/Core/Game/GameIntegrityCheckState";
    import { GameUpdateState } from "@Dodoco/Generated/Dodoco/Core/Game/GameUpdateState";
    import { LauncherDependency } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherDependency";
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Core/Launcher/Settings/LauncherSettings";
    import { Release } from "@Dodoco/Generated/Dodoco/Core/Network/Api/Github/Repos/Release/Release";
    import { WinePackageManagerState } from "@Dodoco/Generated/Dodoco/Core/Wine/WinePackageManagerState";
    import { WineController } from "@Dodoco/Generated/Dodoco/Application/Control/WineController";
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Application/Control/LauncherController";
    import { GameController } from "@Dodoco/Generated/Dodoco/Application/Control/GameController";

    let mainWrapper: HTMLDivElement;
    let settingsModal: ModalControl;
    let confirmGameDownload: ConfirmPopupControl;
    let latestWineRelease: Promise<Release> = WineController.GetControllerInstance().GetLatestRelease();
    let preUpdating: boolean = false;

    onMount(async () => {

        // Set launcher's background image

        mainWrapper.style.backgroundImage = "url('data:image/png;base64," + await LauncherController.GetControllerInstance().GetLauncherBackgroundImage() + "')";

        confirmGameDownload.OnChoose.Add(async (s, e) => {

            if (e) {

                try {

                    await GameController.GetControllerInstance().Download();

                } catch (err: any) {

                    ErrorHandler.PushError(err);

                }

            }

        });

    });

    $: GetProgress = (): number => {

        if ($_MainViewData._ProgressReport == null)
            return 0;

        return Math.floor(($_MainViewData._ProgressReport.Done / $_MainViewData._ProgressReport.Total) * 100);

    }

    $: GetRate = (): string => {

        return DataUnitFormatter.Format($_MainViewData._ProgressReport?.Rate ?? 0) + "/s";

    }

    $: GetEta = (): string => {

        if ($_MainViewData._ProgressReport) {

            if ($_MainViewData._ProgressReport.EstimatedRemainingTime)
                return ($_MainViewData._ProgressReport.EstimatedRemainingTime as string).split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":");

        }

        return "";

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
                                {#if !$_UiStatesHelpers.GameIsDownloading }
                                    { $i18nInstance.t("main.message.dependency.game_download") }
                                {/if}
                            {:else if $_LauncherDependency == LauncherDependency.GAME_UPDATE }
                                {#if !$_UiStatesHelpers.GameIsUpdating }
                                    { $i18nInstance.t("main.message.dependency.game_update") }
                                {/if}
                            {:else if $_LauncherDependency == LauncherDependency.WINE_CONFIGURATION }
                                { $i18nInstance.t("main.message.dependency.wine_configuration") }
                            {:else if $_LauncherDependency == LauncherDependency.WINE_DOWNLOAD }
                                {#if !$_UiStatesHelpers.WinePackageManagerIsWorking }
                                    { $i18nInstance.t("main.message.dependency.wine_download") }
                                {/if}
                            {:else}
                                { $i18nInstance.t("main.message.dependency.other") }
                            {/if}
                        {/if}
                        {#if $_UiStatesHelpers.GameIsDownloading }
                            {#if $_GameDownloadState == GameDownloadState.RECOVERING_DOWNLOADED_SEGMENTS }
                                { $i18nInstance.t("main.message.game_download_state.recovering_downloaded_segments", { percentage: GetProgress() }) }
                            {:else if $_GameDownloadState == GameDownloadState.DOWNLOADING_SEGMENTS }
                                { $i18nInstance.t("main.message.game_download_state.downloading_segments", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetRate() }) }
                            {:else if $_GameDownloadState == GameDownloadState.UNZIPPING_SEGMENTS }
                                { $i18nInstance.t("main.message.game_download_state.unzipping_segments", { percentage: GetProgress() }) }
                            {/if}
                        {/if}
                        {#if $_UiStatesHelpers.GameIsUpdating }
                            {#if $_GameUpdateState == GameUpdateState.DOWNLOADING_UPDATE_PACKAGE }
                                { $i18nInstance.t("main.message.game_update_state.downloading_update_package", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetRate() }) }
                            {:else if $_GameUpdateState == GameUpdateState.UNZIPPING_UPDATE_PACKAGE }
                                { $i18nInstance.t("main.message.game_update_state.unzipping_update_package", { percentage: GetProgress() }) }
                            {:else if $_GameUpdateState == GameUpdateState.APPLYING_UPDATE_PACKAGE }
                                { $i18nInstance.t("main.message.game_update_state.applying_update_package", { percentage: GetProgress() }) }
                            {:else if $_GameUpdateState == GameUpdateState.REMOVING_DEPRECATED_FILES }
                                { $i18nInstance.t("main.message.game_update_state.removing_deprecated_files", { percentage: GetProgress() }) }
                            {/if}
                        {/if}
                        {#if $_UiStatesHelpers.GameIsCheckingIntegrity }
                            {#if $_GameIntegrityCheckState == GameIntegrityCheckState.CHECKING_INTEGRITY }
                                { $i18nInstance.t("main.message.game_integrity_check_state.checking_integrity", { percentage: GetProgress(), remaining_time: GetEta() }) }
                            {:else if $_GameIntegrityCheckState == GameIntegrityCheckState.DOWNLOADING_FILE }
                                { $i18nInstance.t("main.message.game_integrity_check_state.downloading_file", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetRate() }) }
                            {:else if $_GameIntegrityCheckState == GameIntegrityCheckState.REPAIRING_FILE }
                                { $i18nInstance.t("main.message.game_integrity_check_state.repairing_file") }
                            {/if}
                        {/if}
                        {#if $_UiStatesHelpers.WinePackageManagerIsWorking }
                            {#if $_WinePackageManagerState == WinePackageManagerState.DOWNLOADING_PACKAGE }
                                { $i18nInstance.t("main.message.wine_package_manager_state.downloading_package", { percentage: GetProgress(), remaining_time: GetEta(), download_rate: GetRate() }) }
                            {:else if $_WinePackageManagerState == WinePackageManagerState.DECOMPRESSING_PACKAGE }
                                { $i18nInstance.t("main.message.wine_package_manager_state.decompressing_package") }
                            {:else if $_WinePackageManagerState == WinePackageManagerState.CHECKING_PACKAGE_CHECKSUM }
                                { $i18nInstance.t("main.message.wine_package_manager_state.checking_package_checksum") }
                            {/if}
                        {/if}
                        {#if $_UiStatesHelpers.LauncherIsBusy && $_MainViewData._ProgressReport?.Message }
                            <br>{$_MainViewData._ProgressReport.Message}
                        {/if}
                    </small>
                    {#if $_UiStatesHelpers.LauncherIsBusy && GetProgress() != 0 }
                        <ProgressBar value={GetProgress()} width="1/4"/>
                    {/if}
                    <ButtonGroup>
                        <Button on:click={() => settingsModal.Open() } disabled={ $_UiStatesHelpers.LauncherIsBusy }><Icon icon="material-symbols:settings"/></Button>
                        {#await GameController.GetControllerInstance().GetPreUpdateAsync() then preUpdate}
                            {#if preUpdate}
                                {#await GameController.GetControllerInstance().IsPreUpdateDownloadedAsync() then preUpdateDownloaded}
                                    {#if !preUpdateDownloaded}
                                        <Button on:click={async (e) => {

                                            preUpdating = true;
                                            try { await GameController.GetControllerInstance().UpdateAsync(true); }
                                            catch (err) { ErrorHandler.PushError(err); }
                                            finally { preUpdating = false; }

                                        }} disabled={ $_UiStatesHelpers.GameIsUpdating }>
                                            <Icon icon="material-symbols:deployed-code-update-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.pre_update_game") }
                                        </Button>
                                    {/if}
                                {/await}
                            {/if}
                        {/await}
                        {#if $_LauncherDependency == LauncherDependency.WINE_DOWNLOAD }
                            {#await latestWineRelease then result}
                                <Button focused disabled={ $_UiStatesHelpers.WinePackageManagerIsWorking } on:click={async () => {

                                    try {

                                        await WineController.GetControllerInstance().InstallLatestRelease();
                                        let settings = await LauncherController.GetControllerInstance().GetLauncherSettings();
                                        settings.Wine.SelectedRelease = (await latestWineRelease).tag_name;
                                        await LauncherController.GetControllerInstance().SetLauncherSettings(settings);

                                    } catch (err) { ErrorHandler.PushError(err); }

                                }}>
                                    <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.download_wine", { version: result.tag_name } ) }
                                </Button>
                            {/await}
                        {:else if $_LauncherDependency == LauncherDependency.GAME_DOWNLOAD }
                            <Button focused disabled={ $_UiStatesHelpers.GameIsDownloading } on:click={() => {

                                try { confirmGameDownload.Open(); }
                                catch (err) { ErrorHandler.PushError(err); }

                            }}>
                                <Icon icon="material-symbols:cloud-download-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.download_game") }
                            </Button>
                        {:else if $_LauncherDependency == LauncherDependency.GAME_UPDATE }
                            <Button focused disabled={ $_UiStatesHelpers.GameIsUpdating } on:click={async () => {

                                try { await GameController.GetControllerInstance().UpdateAsync(false); }
                                catch (err) { ErrorHandler.PushError(err); }

                            }}>
                                <Icon icon="material-symbols:deployed-code-update-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.update_game") }
                            </Button>
                        {:else if $_LauncherDependency == LauncherDependency.NONE }
                            <Button focused disabled={ $_UiStatesHelpers.LauncherIsBusy && !preUpdating } on:click={async () => {

                                try { await GameController.GetControllerInstance().Start(); }
                                catch (err) { ErrorHandler.PushError(err); }

                            }}>
                                <Icon icon="material-symbols:play-arrow-rounded" />&nbsp;{ $i18nInstance.t("main.button.main.start_game") }
                            </Button>
                        {:else}
                            <Button focused disabled>
                                <Icon icon="material-symbols:autorenew-outline-rounded" class="animate-spin"/>&nbsp;Unready
                            </Button>
                        {/if}
                    </ButtonGroup>
                </div>
            </div>
        </div>
    </div>
    <ConfirmPopup bind:Root={confirmGameDownload} callback={(e) => null}>
        {#await LauncherController.GetControllerInstance().GetLauncherSettings() then result }
            <p>{ $i18nInstance.t("main.confirm.game_download.text", { directory: result.Game.InstallationDirectory, menu_path: `${$i18nInstance.t("settings.title")} > ${$i18nInstance.t("settings.menu.game")}` }) }</p>
        {/await}
    </ConfirmPopup>
    <Settings bind:Root={settingsModal}/>
</div>
<style>
</style>