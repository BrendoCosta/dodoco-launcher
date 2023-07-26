<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { _GameState, _LauncherDependency, _LauncherState, _WineControllerViewData, _WinePackageManagerState, i18nInstance } from "@Dodoco/Global";
    import { Checkbox, ConfirmPopup, ConfirmPopupControl, LoadingDots, Modal, ModalControl, Popup, Radio, RadioGroup, ScoopedFrame } from "@Dodoco/Components";
    import { LanguageConstants, LanguageName } from "@Dodoco/Language";
    
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    
    import Icon from "@iconify/svelte";
    import { onMount } from "svelte";
    import { Writable, writable } from "svelte/store";
    import { CommonErrorData } from "..";

    // Generated types
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Core/Launcher/Settings/LauncherSettings";
    import { Release } from "@Dodoco/Generated/Dodoco/Core/Network/Api/Github/Repos/Release/Release";
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Application/Control/LauncherController";
    import { WineController } from "@Dodoco/Generated/Dodoco/Application/Control/WineController";
    import { GameController } from "@Dodoco/Generated/Dodoco/Application/Control/GameController";
    import { LauncherDependency } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherDependency";
    import { DataUnitFormatter } from "@Dodoco/Util";
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { WinePackageManagerState } from "@Dodoco/Generated/Dodoco/Core/Wine/WinePackageManagerState";

    export let Root: ModalControl = new ModalControl();
    let errorModalControl: ConfirmPopupControl;
    let errorData: CommonErrorData = { code: 0 };
    let confirmRestoreSettings: ConfirmPopupControl;
    let confirmSaveSettings: ConfirmPopupControl;
    let userSettingsPromise: Promise<LauncherSettings>;
    let userSettings: Writable<LauncherSettings>;
    let installedWineTags: string[] = [];
    let installedWineTagsRadioGroup: RadioGroup<string>;
    let avaliableWineReleases: Promise<Release[]>;

    onMount(async () => {

        // Load the current settings whenever modal opens

        Root.OnOpen.Add(async (sender, args) => {

            await LoadData();

        });

    });

    $: LauncherIsBusy = (): boolean => {

        return ($_GameState ?? GameState.READY) != GameState.READY
        || ($_WinePackageManagerState ?? WinePackageManagerState.READY) != WinePackageManagerState.READY;

    }

    async function LoadData(): Promise<void> {

        try {

            userSettingsPromise = LauncherController.GetControllerInstance().GetLauncherSettings();
            userSettings = writable(await userSettingsPromise);
            installedWineTags = await WineController.GetControllerInstance().GetInstalledTags();
            installedWineTagsRadioGroup = new RadioGroup<string>(installedWineTags);
            avaliableWineReleases = WineController.GetControllerInstance().GetAvaliableReleases();

        } catch (err: any) {

            if (err.data) {

                errorData = err.data as CommonErrorData;
                errorModalControl.Open();
                return;

            }

        }

    }

    async function DownloadWine(release: Release): Promise<void> {

        await WineController.GetControllerInstance().InstallPackageFromRelease(release);
        await LoadData();

    }

    async function RepairGame() {

        try {

            await GameController.GetControllerInstance().RepairGameFiles();
            
        } catch (err: any) {

            if (err.data) {

                errorData = err.data as CommonErrorData;
                errorModalControl.Open();
                return;

            }

        }
        
    }

</script>
<Modal bind:Root={Root} closable={!LauncherIsBusy()}>
    <ScoopedFrame width="[60%]" height="[80%]">
        <div class="w-full h-full flex flex-col items-start gap-y-10 m-2">
            <h1 class="text-3xl font-medium text-gray-800 drop-shadow-md">✦ { $i18nInstance.t("settings.title") }</h1>
            <div class="w-full h-full overflow-y-scroll">
                {#await userSettingsPromise}
                    Loading settings
                {:then us}
                    <TabGroup class="flex flex-row gap-x-20 p-2">
                        <TabList class="flex flex-col items-start gap-y-4">
                            <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ $i18nInstance.t("settings.menu.general") }</h2></Tab>
                            <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ $i18nInstance.t("settings.menu.game") }</h2></Tab>
                            <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>Wine</h2></Tab>
                        </TabList>
                        <TabPanels class="w-full">
                            <TabPanel class="panel">
                                <ul>
                                    <li>
                                        <h3>{ $i18nInstance.t("settings.content.general.default_settings.title") }</h3>
                                        <p>{ $i18nInstance.t("settings.content.general.default_settings.description") }</p>
                                        <button class="input" on:click={() => confirmRestoreSettings.Open()}>
                                            { $i18nInstance.t("settings.content.general.default_settings.button_text") }
                                        </button>
                                    </li>
                                    <li>
                                        <h3>{ $i18nInstance.t("settings.content.general.language.title") }</h3>
                                        <Listbox bind:value={$userSettings.Launcher.Language} on:change={(e) => { $userSettings.Launcher.Language = e.detail; }}>
                                            <ListboxButton class="input listbox-button">
                                                { LanguageName.get($userSettings.Launcher.Language) }<Icon icon="material-symbols:arrow-drop-down"/>
                                            </ListboxButton>
                                            <ListboxOptions class="input dropdown">
                                                {#each LanguageConstants.SupportedLanguages as lang }
                                                <ListboxOption class="item" value={lang}>
                                                    { LanguageName.get(lang) }
                                                </ListboxOption>
                                                {/each}
                                            </ListboxOptions>
                                        </Listbox>                                              
                                    </li>
                                    <li>
                                        <h3>{ $i18nInstance.t("settings.content.general.update.title") }</h3>
                                        <p>{ $i18nInstance.t("settings.content.general.update.description", { version: "0.0.0" }) }</p>
                                        <button class="input">{ $i18nInstance.t("settings.content.general.update.button_text") }</button>
                                        <div class="inline-flex items-center">
                                            <Checkbox bind:checked={$userSettings.Launcher.AutoSearchForUpdates}/><p>&nbsp;{ $i18nInstance.t("settings.content.general.update.auto_search_for_updates") }</p>
                                        </div>
                                    </li>
                                </ul>
                            </TabPanel>
                            <TabPanel class="panel">
                                <ul>
                                    <li>
                                        <h3>{ $i18nInstance.t("settings.content.game.integrity.title") }</h3>
                                        <p>{ $i18nInstance.t("settings.content.game.integrity.description") }</p>
                                        <button disabled={$_LauncherDependency == LauncherDependency.GAME_DOWNLOAD } class="input" on:click={() => {
                                            Root.Close();
                                            RepairGame();
                                        }}>
                                            { $i18nInstance.t("settings.content.game.integrity.button_text") }
                                        </button>
                                    </li>
                                    <li>
                                        <h3>{ $i18nInstance.t("settings.content.game.installation_directory.title") }</h3>
                                        <input bind:value={$userSettings.Game.InstallationDirectory} type="text" class="input text"/>
                                    </li>
                                </ul>
                            </TabPanel>
                            <TabPanel class="panel">
                                <ul>
                                    <li>
                                        <h3>Installation Mode</h3>
                                        <div class="inline-flex items-center">
                                            <Checkbox bind:checked={$userSettings.Wine.UserDefinedInstallation}/><p>&nbsp;Use a local Wine installation</p>
                                        </div>
                                    </li>
                                    <li>
                                        {#if $userSettings.Wine.UserDefinedInstallation }
                                            <h3>Installation Directory</h3>
                                            <p>Path to your Wine installation directory</p>
                                            <input bind:value={$userSettings.Wine.InstallationDirectory} type="text" class="input text"/>
                                        {:else}
                                            <h3>Releases Directory</h3>
                                            <p>Path to Launcher's Wine's releases directory</p>
                                            <input bind:value={$userSettings.Wine.ReleasesDirectory} type="text" class="input text"/>
                                        {/if}
                                    </li>
                                    <li>
                                        <h3>Prefix</h3>
                                        <p>Path to Wine's prefix directory</p>
                                        <input bind:value={$userSettings.Wine.PrefixDirectory} type="text" class="input text"/>
                                    </li>
                                    {#if !$userSettings.Wine.UserDefinedInstallation }
                                        <li>
                                            <h3>{ $i18nInstance.t("settings.content.wine.version.title") }</h3>
                                            <div class="w-full flex flex-col items-center justify-center gap-y-4">
                                                {#await avaliableWineReleases }
                                                    <p>{ $i18nInstance.t("settings.content.wine.version.loading_avaliable_releases") }<LoadingDots/></p>
                                                {:then releaseList } 
                                                    {#each releaseList as release }
                                                        <div class="inline-flex w-full items-center">
                                                            <div class="w-[70%] flex flex-col items-start">
                                                                <p>{ release.tag_name }</p>
                                                                <small class="text-gray-800/50 text-left">
                                                                    {#if release.tag_name in $_WineControllerViewData.ReleaseDownloadProgressReport }
                                                                        {   $i18nInstance.t("settings.content.wine.version.downloading_release", {
                                                                                percentage: Math.floor($_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].CompletionPercentage),
                                                                                remaining_time: $_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].EstimatedRemainingTime.toString().split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":"),
                                                                                download_rate: DataUnitFormatter.Format($_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].BytesPerSecond ?? 0)  + "/s"
                                                                            })
                                                                        }
                                                                    {:else}
                                                                        {@const packageAsset = release.assets.find(a => new RegExp(".*\\.(7z|gz|xz|tar|zip)$").test(a.name)) }
                                                                        {#if packageAsset }
                                                                            { $i18nInstance.t("settings.content.wine.version.date", { date: release.published_at }) }
                                                                            <br>{ $i18nInstance.t("settings.content.wine.version.filename", { filename: packageAsset.name }) }
                                                                            <br>{ $i18nInstance.t("settings.content.wine.version.filesize", { filesize: DataUnitFormatter.Format(packageAsset.size) }) }
                                                                            <br>{ $i18nInstance.t("settings.content.wine.version.downloads_count", { downloads_count: packageAsset.download_count }) }
                                                                        {/if}
                                                                    {/if}
                                                                </small>
                                                            </div>
                                                            <div class="w-[30%] flex flex-col items-center">
                                                                {#if installedWineTags.find(tag => tag == release.tag_name ) }
                                                                    <Radio on:click={() => $userSettings.Wine.SelectedRelease = installedWineTagsRadioGroup.Selected ?? $userSettings.Wine.SelectedRelease } bind:group={installedWineTagsRadioGroup} value={release.tag_name} selected={ $userSettings.Wine.SelectedRelease == release.tag_name }/>
                                                                {:else}
                                                                    {#if release.tag_name in $_WineControllerViewData.ReleaseDownloadProgressReport }
                                                                        <Icon icon="material-symbols:autorenew-outline-rounded" class="animate-spin"/>
                                                                    {:else}
                                                                        <button class="input" on:click={() => DownloadWine(release)}>{ $i18nInstance.t("settings.content.wine.version.download") }</button>
                                                                    {/if}
                                                                {/if}
                                                            </div>
                                                        </div>
                                                    {/each}
                                                {:catch error } 
                                                    <p>Failed to load Wine's release list</p>
                                                {/await}
                                            </div>
                                        </li>
                                    {/if}
                                </ul>
                            </TabPanel>
                        </TabPanels>
                    </TabGroup>
                {/await}
            </div>
            <div class="w-full">
                <div class="w-2/3 inline-flex justify-center gap-x-6">
                    <button class="input" disabled={LauncherIsBusy()} on:click={() => { confirmSaveSettings.Open(); }}>{ $i18nInstance.t("settings.button.save_settings") }</button>
                </div>
            </div>
        </div>
    </ScoopedFrame>
</Modal>
<ConfirmPopup bind:Root={confirmRestoreSettings} callback={(e) => { console.log(e) }}>
    <p>Restore default settings?</p>
</ConfirmPopup>
<ConfirmPopup bind:Root={confirmSaveSettings} callback={async (e) => {
    
    if (e) {

        // Write the settings to the storage
        await LauncherController.GetControllerInstance().SetLauncherSettings($userSettings);
        
        // Changes the UI language
        $i18nInstance.changeLanguage($userSettings.Launcher.Language);

    }

}}>
    <p>{ $i18nInstance.t("settings.confirm.save_settings") }</p>
</ConfirmPopup>
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
<style lang="postcss">

    .panel {

        @apply w-full flex flex-col justify-start items-start

    }

    ul {

        @apply w-full flex flex-col items-start gap-y-6

    }

    .panel ul li {

        @apply w-full flex flex-col items-start gap-y-2 text-gray-800

    }

    .panel ul li h3 {

        @apply text-xl drop-shadow-md

    }

    .panel ul li p {

        @apply text-sm text-left

    }

    .panel ul li .input.text {

        @apply w-[90%]

    }

</style>