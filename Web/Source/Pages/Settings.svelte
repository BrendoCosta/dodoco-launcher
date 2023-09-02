<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { ErrorHandler } from "@Dodoco/index";
    import { _ConfirmPopup, _GameState, _LauncherDependency, _LauncherState, _WineControllerViewData, _WinePackageManagerState, i18nInstance } from "@Dodoco/Global";
    import { Checkbox, LoadingDots, Modal, ModalControl, Radio, RadioGroup, ScoopedFrame } from "@Dodoco/Components";
    import { LanguageConstants, LanguageName } from "@Dodoco/Language";
    
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    
    import Icon from "@iconify/svelte";
    import { onMount } from "svelte";
    import { Writable, writable } from "svelte/store";
    import sanitizeHtml from 'sanitize-html';

    // Generated types
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Core/Launcher/Settings/LauncherSettings";
    import { Release } from "@Dodoco/Generated/Dodoco/Core/Network/Api/Github/Repos/Release/Release";
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Application/Control/LauncherController";
    import { WineController } from "@Dodoco/Generated/Dodoco/Application/Control/WineController";
    import { GameController } from "@Dodoco/Generated/Dodoco/Application/Control/GameController";
    import { DataUnitFormatter } from "@Dodoco/Util";
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    import { WinePackageManagerState } from "@Dodoco/Generated/Dodoco/Core/Wine/WinePackageManagerState";
    import { GameServer } from "@Dodoco/Generated/Dodoco/Core/Game/GameServer";
    import { LauncherDependency } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherDependency";
    import TextInput from "@Dodoco/Components/TextInput.svelte";

    export let Root: ModalControl = new ModalControl();
    let userSettingsPromise: Promise<LauncherSettings>;
    let userSettings: Writable<LauncherSettings>;
    let installedWineTags: string[] = [];
    let installedWineTagsRadioGroup: RadioGroup<string>;
    let avaliableWineReleases: Promise<Release[]>;
    let avaliableServersList: Promise<GameServer[]>;

    onMount(async () => {

        // Load the current settings whenever modal opens

        Root.OnOpen.Add(async (sender, args) => {

            await LoadData();

        });

    });

    $: LauncherIsWaiting = (): boolean => {

        return ($_LauncherDependency ?? LauncherDependency.NONE) != LauncherDependency.NONE;

    }

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
            avaliableServersList = GameController.GetControllerInstance().GetAvaliableGameServers();

        } catch (err: any) {

            ErrorHandler.PushError(err);

        }

    }

    async function DownloadWine(release: Release): Promise<void> {

        try {

            await WineController.GetControllerInstance().InstallPackageFromRelease(release);
            await LoadData();

        } catch (err: any) {

            ErrorHandler.PushError(err);

        }

    }

    async function RepairGame() {

        try {

            await GameController.GetControllerInstance().RepairGameFiles();
            
        } catch (err: any) {

            ErrorHandler.PushError(err);

        }
        
    }

</script>
<Modal bind:Root={Root} closable={!LauncherIsBusy()}>
    <ScoopedFrame width="[60%]" height="[80%]">
        <div class="w-full h-full flex flex-col items-start gap-y-10 m-2">
            <h1 class="text-3xl hy-impact-font font-medium text-zinc-700 drop-shadow-md">✦ { $i18nInstance.t("settings.title") }</h1>
            <div class="w-full h-full overflow-y-scroll">
                {#await userSettingsPromise}
                    Loading settings
                {:then us}
                    <TabGroup class="flex flex-row gap-x-20 p-2">
                        <TabList class="flex flex-col items-start gap-y-4 hy-impact-font font-medium drop-shadow-md">
                            <Tab class={(e) => e.selected ? "text-zinc-700" : "[&>h2]:text-zinc-700/50"}><h2>{ $i18nInstance.t("settings.menu.general") }</h2></Tab>
                            <Tab class={(e) => e.selected ? "text-zinc-700" : "[&>h2]:text-zinc-700/50"}><h2>{ $i18nInstance.t("settings.menu.game") }</h2></Tab>
                            <Tab class={(e) => e.selected ? "text-zinc-700" : "[&>h2]:text-zinc-700/50"}><h2>Wine</h2></Tab>
                        </TabList>
                        <TabPanels class="w-full pe-2">
                            <TabPanel class="panel">
                                <div data-role="form">
                                    <ul>
                                        <li>
                                            <h3>{ $i18nInstance.t("settings.content.general.default_settings.title") }</h3>
                                            <p>{ $i18nInstance.t("settings.content.general.default_settings.description") }</p>
                                            <button data-role="button" on:click={() => {

                                                _ConfirmPopup.update(arr => { arr.push({
                                                    text: "Restore default settings?",
                                                    callback: async (e) => {
                                                        if (e) {

                                                            // TODO

                                                        }
                                                    }
                                                }); return arr })

                                            }}>
                                                { $i18nInstance.t("settings.content.general.default_settings.button_text") }
                                            </button>
                                        </li>
                                        <li>
                                            <h3>{ $i18nInstance.t("settings.content.general.language.title") }</h3>
                                            <Listbox class="relative" bind:value={$userSettings.Launcher.Language} on:change={(e) => { $userSettings.Launcher.Language = e.detail; }}>
                                                <ListboxButton class="listbox-button">
                                                    { LanguageName.get($userSettings.Launcher.Language) }<Icon icon="material-symbols:arrow-drop-down"/>
                                                </ListboxButton>
                                                <ListboxOptions class="listbox-dropdown">
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
                                            <button data-role="button">{ $i18nInstance.t("settings.content.general.update.button_text") }</button>
                                            <div class="inline-flex items-center">
                                                <Checkbox bind:checked={$userSettings.Launcher.AutoSearchForUpdates}/><p>&nbsp;{ $i18nInstance.t("settings.content.general.update.auto_search_for_updates") }</p>
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </TabPanel>
                            <TabPanel class="panel">
                                <div data-role="form">
                                    <ul>
                                        <li>
                                            <h3>{$i18nInstance.t("settings.content.game.installation_directory.title")}</h3>
                                            <input data-role="input text" bind:value={$userSettings.Game.InstallationDirectory} type="text" spellcheck="false"/>
                                        </li>
                                        <li>
                                            <h3>{ $i18nInstance.t("settings.content.game.server.title") }</h3>
                                            <p>{ @html sanitizeHtml($i18nInstance.t("settings.content.game.server.warning")) }</p>
                                            {#await avaliableServersList then gameServerlist }
                                                <Listbox class="relative" bind:value={$userSettings.Game.Server}>
                                                    <ListboxButton class="listbox-button">
                                                        { GameServer[$userSettings.Game.Server] }<Icon icon="material-symbols:arrow-drop-down"/>
                                                    </ListboxButton>
                                                    <ListboxOptions class="listbox-dropdown">
                                                        {#each gameServerlist as server }
                                                            <ListboxOption class="item" value={server}>
                                                                { GameServer[server] }
                                                            </ListboxOption>
                                                        {/each}
                                                    </ListboxOptions>
                                                </Listbox>
                                            {/await}
                                        </li>
                                        <li>
                                            <h3>{ $i18nInstance.t("settings.content.game.integrity.title") }</h3>
                                            <p>{ $i18nInstance.t("settings.content.game.integrity.description") }</p>
                                            <button data-role="button" disabled={ LauncherIsBusy() || LauncherIsWaiting() } on:click={() => {
                                                Root.Close();
                                                RepairGame();
                                            }}>
                                                { $i18nInstance.t("settings.content.game.integrity.button_text") }
                                            </button>
                                        </li>
                                    </ul>
                                </div>
                            </TabPanel>
                            <TabPanel class="panel">
                                <div data-role="form">
                                    <ul>
                                        <li>
                                            <h3>Installation Mode</h3>
                                            <section>
                                                <Radio bind:selected={$userSettings.Wine.UserDefinedInstallation} value={false}/>
                                                <label>Managed</label>
                                            </section>
                                            <section>
                                                <Radio bind:selected={$userSettings.Wine.UserDefinedInstallation} value={true}/>
                                                <label>Custom</label>
                                            </section>
                                        </li>
                                        <li>
                                            {#if $userSettings.Wine.UserDefinedInstallation }
                                                <h3>Installation Directory</h3>
                                                <input data-role="input text" bind:value={$userSettings.Wine.InstallationDirectory} type="text" spellcheck="false"/>
                                            {:else}
                                                <h3>Releases Directory</h3>
                                                <input data-role="input text" bind:value={$userSettings.Wine.ReleasesDirectory} type="text" spellcheck="false"/>
                                            {/if}
                                        </li>
                                        <li>
                                            <h3>Prefix Directory</h3>
                                            <input data-role="input text" bind:value={$userSettings.Wine.PrefixDirectory} type="text" spellcheck="false"/>
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
                                                                    <small class="text-zinc-700/50 text-left">
                                                                        {#if release.tag_name in $_WineControllerViewData.ReleaseDownloadProgressReport }
                                                                            {   $i18nInstance.t("settings.content.wine.version.downloading_release", {
                                                                                    percentage: Math.floor(($_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].Done / $_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].Total) * 100),
                                                                                    remaining_time: $_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].EstimatedRemainingTime != null ? $_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].EstimatedRemainingTime.toString().split(":").map(e => parseInt(e).toString().padStart(2, "0")).join(":") : "",
                                                                                    download_rate: DataUnitFormatter.Format($_WineControllerViewData.ReleaseDownloadProgressReport[release.tag_name].Rate ?? 0)  + "/s"
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
                                                                        <Radio bind:selected={$userSettings.Wine.SelectedRelease} value={release.tag_name}/>
                                                                    {:else}
                                                                        {#if release.tag_name in $_WineControllerViewData.ReleaseDownloadProgressReport }
                                                                            <Icon icon="material-symbols:autorenew-outline-rounded" class="animate-spin"/>
                                                                        {:else}
                                                                            <button data-role="button" on:click={() => DownloadWine(release)}>{ $i18nInstance.t("settings.content.wine.version.download") }</button>
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
                                </div>
                            </TabPanel>
                        </TabPanels>
                    </TabGroup>
                {/await}
            </div>
            <div class="w-full">
                <div class="w-2/3 inline-flex justify-center gap-x-6">
                    <button data-role="button" data-highlight disabled={LauncherIsBusy()} on:click={() => {
                        _ConfirmPopup.update(arr => { arr.push({
                            text: $i18nInstance.t("settings.confirm.save_settings"),
                            callback: async (e) => {
                                if (e) {

                                    // Write the settings to the storage
                                    await LauncherController.GetControllerInstance().SetLauncherSettings($userSettings);

                                    // Changes the UI language
                                    $i18nInstance.changeLanguage($userSettings.Launcher.Language);

                                }
                            }
                        }); return arr })
                    }}>{ $i18nInstance.t("settings.button.save_settings") }</button>
                </div>
            </div>
        </div>
    </ScoopedFrame>
</Modal>
<style lang="postcss">

    .panel {

        @apply w-full flex flex-col justify-start items-start

    }

    ul {

        @apply w-full flex flex-col items-start gap-y-6

    }

    .panel ul li {

        @apply w-full flex flex-col items-start gap-y-2 text-zinc-700

    }

    .panel ul li p {

        @apply text-left

    }

    .panel ul li .input.text {

        @apply w-[90%]

    }

</style>