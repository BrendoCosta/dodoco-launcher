<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { i18nInstance, _SettingsViewData } from "@Dodoco/Global";
    import { Checkbox, ConfirmPopup, ConfirmPopupControl, LoadingDots, Modal, ModalControl, Radio, RadioGroup, ScoopedFrame } from "@Dodoco/Components";
    import { LanguageConstants, LanguageName } from "@Dodoco/Language";
    
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    
    import Icon from "@iconify/svelte";
    import { onMount } from "svelte";
    import { Writable, writable } from "svelte/store";

    // Generated types
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Core/Launcher/Settings/LauncherSettings";
    import { SettingsController } from "@Dodoco/Generated/Dodoco/Application/Control/SettingsController";
    import { Release } from "@Dodoco/Generated/Dodoco/Core/Network/Api/Github/Repos/Release/Release";

    export let Root: ModalControl = new ModalControl();
    let confirmRestoreSettings: ConfirmPopupControl;
    let confirmSaveSettings: ConfirmPopupControl;
    let EditableSettings: Writable<LauncherSettings> = writable({} as LauncherSettings);
    let installedWineTags: string[] = [];
    let installedWineTagsRadioGroup: RadioGroup<string>;
    let avaliableWineReleases: Promise<Release[]>;

    onMount(async () => {

        // Load the current settings whenever modal opens

        Root.OnOpen.Add(async (sender, args) => {

            await RecoveryWine();
            EditableSettings = writable(await SettingsController.GetControllerInstance().GetLauncherSettings());

        });

    });

    async function RecoveryWine(): Promise<void> {

        installedWineTags = await SettingsController.GetControllerInstance().GetInstalledWineTags();
        installedWineTagsRadioGroup = new RadioGroup<string>(installedWineTags);
        avaliableWineReleases = SettingsController.GetControllerInstance().GetAvaliableWineReleases();

    }

    async function DownloadWine(release: Release): Promise<void> {

        await SettingsController.GetControllerInstance().DownloadWine(release);
        await RecoveryWine();

    }

</script>
<Modal bind:Root={Root}>
    <ScoopedFrame width="[60%]" height="[80%]">
        <div class="w-full h-full flex flex-col items-start gap-y-10 m-2">
            <h1 class="text-3xl font-medium text-gray-800 drop-shadow-md">✦ { $i18nInstance.t("settings.title") }</h1>
            <div class="w-full h-full overflow-y-scroll">
                <TabGroup class="flex flex-row gap-x-20 p-2">
                    <TabList class="flex flex-col items-start gap-y-4">
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ $i18nInstance.t("settings.menu.general") }</h2></Tab>
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ $i18nInstance.t("settings.menu.launcher") }</h2></Tab>
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
                            </ul>
                        </TabPanel>
                        <TabPanel class="panel">
                            <ul>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.launcher.update.title") }</h3>
                                    <p>{ $i18nInstance.t("settings.content.launcher.update.description", { version: "0.0.0" }) }</p>
                                    <button class="input">{ $i18nInstance.t("settings.content.launcher.update.button_text") }</button>
                                    <div class="inline-flex items-center">
                                        <Checkbox bind:checked={$EditableSettings.Launcher.AutoSearchForUpdates}/><p>&nbsp;{ $i18nInstance.t("settings.content.launcher.update.auto_search_for_updates") }</p>
                                    </div>
                                </li>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.launcher.language.title") }</h3>
                                    <Listbox value={$EditableSettings.Launcher.Language} on:change={(e) => { $EditableSettings.Launcher.Language = e.detail; }}>
                                        <ListboxButton class="input listbox-button">
                                            { LanguageName.get($EditableSettings.Launcher.Language) }<Icon icon="material-symbols:arrow-drop-down"/>
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
                            </ul>
                        </TabPanel>
                        <TabPanel class="panel">
                            <ul>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.game.integrity.title") }</h3>
                                    <p>{ $i18nInstance.t("settings.content.game.integrity.description") }</p>
                                    <button class="input" on:click={() => {
                                        Root.Close();
                                        SettingsController.GetControllerInstance().CheckFilesIntegrity();
                                    }}>
                                        { $i18nInstance.t("settings.content.game.integrity.button_text") }
                                    </button>
                                </li>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.game.installation_directory.title") }</h3>
                                    <input bind:value={$EditableSettings.Game.InstallationDirectory} type="text" class="input text"/>
                                </li>
                            </ul>
                        </TabPanel>
                        <TabPanel class="panel">
                            <ul>
                                <li>
                                    <h3>Installation Mode</h3>
                                    <div class="inline-flex items-center">
                                        <Checkbox bind:checked={$EditableSettings.Wine.UserDefinedInstallation}/><p>&nbsp;Use a local Wine installation</p>
                                    </div>
                                </li>
                                <li>
                                    {#if $EditableSettings.Wine.UserDefinedInstallation }
                                        <h3>Installation Directory</h3>
                                        <p>Path to your Wine installation directory</p>
                                        <input bind:value={$EditableSettings.Wine.InstallationDirectory} type="text" class="input text"/>
                                    {:else}
                                        <h3>Releases Directory</h3>
                                        <p>Path to Launcher's Wine's releases directory</p>
                                        <input bind:value={$EditableSettings.Wine.ReleasesDirectory} type="text" class="input text"/>
                                    {/if}
                                </li>
                                <li>
                                    <h3>Prefix</h3>
                                    <p>Path to Wine's prefix directory</p>
                                    <input bind:value={$EditableSettings.Wine.PrefixDirectory} type="text" class="input text"/>
                                </li>
                                {#if !$EditableSettings.Wine.UserDefinedInstallation }
                                    <li>
                                        <h3>Version</h3>
                                        <div class="w-full flex flex-col items-center justify-center gap-y-4">
                                            {#await avaliableWineReleases }
                                                <p>Loading...</p>
                                            {:then releaseList } 
                                                {#each releaseList as release }
                                                    <div class="inline-flex w-full items-center">
                                                        <div class="w-[70%] flex flex-col items-start">
                                                            <p>{ release.tag_name }</p>
                                                            <small class="text-gray-800/50">
                                                                {#if $_SettingsViewData.WineDownloadStatus[release.tag_name] }
                                                                    Downloading release<LoadingDots/> {Math.floor($_SettingsViewData.WineDownloadStatus[release.tag_name].CompletionPercentage)}% (ETA: {$_SettingsViewData.WineDownloadStatus[release.tag_name].EstimatedRemainingTime.toString().split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":")})
                                                                {:else}
                                                                    { release.published_at }
                                                                {/if}
                                                            </small>
                                                        </div>
                                                        <div class="w-[30%] flex flex-col items-center">
                                                            {#if installedWineTags.find(tag => tag == release.tag_name ) }
                                                                <Radio on:click={() => $EditableSettings.Wine.SelectedRelease = installedWineTagsRadioGroup.Selected ?? $EditableSettings.Wine.SelectedRelease } bind:group={installedWineTagsRadioGroup} value={release.tag_name} selected={ $EditableSettings.Wine.SelectedRelease == release.tag_name }/>
                                                            {:else}
                                                                {#if $_SettingsViewData.WineDownloadStatus[release.tag_name] }
                                                                    <Icon icon="material-symbols:autorenew-outline-rounded" class="animate-spin"/>
                                                                {:else}
                                                                    <button class="input" on:click={() => DownloadWine(release)}>Download</button>
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
            </div>
            <div class="w-full">
                <div class="w-2/3 inline-flex justify-center gap-x-6">
                    <button class="input" on:click={() => { confirmSaveSettings.Open(); }}>{ $i18nInstance.t("settings.button.save_settings") }</button>
                </div>
            </div>
        </div>
    </ScoopedFrame>
</Modal>
<p>ASTA</p>
<ConfirmPopup bind:Root={confirmRestoreSettings} callback={(e) => { console.log(e) }}>
    <p>Restore default settings?</p>
</ConfirmPopup>
<ConfirmPopup bind:Root={confirmSaveSettings} callback={async (e) => {
    
    if (e) {

        // Write the settings to the storage
        await SettingsController.GetControllerInstance().SetLauncherSettings($EditableSettings);
        
        // Changes the UI language
        $i18nInstance.changeLanguage($EditableSettings.Launcher.Language);

    }

}}>
    <p>{ $i18nInstance.t("settings.confirm.save_settings") }</p>
</ConfirmPopup>
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