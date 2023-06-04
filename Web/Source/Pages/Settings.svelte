<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { i18nInstance } from "@Dodoco/GlobalInstances";
    import { Checkbox, Modal, ModalControl, ScoopedFrame } from "@Dodoco/Components";
    import { Language, LanguageConstants, LanguageName } from "@Dodoco/Language";
    
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    import Icon from "@iconify/svelte";

    // Generated types
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Network/Controller/LauncherController";
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Launcher/Settings/LauncherSettings";

    $: _i18n = $i18nInstance;
    
    export let Root: ModalControl;
    export let settings: LauncherSettings;

    let selectedLanguage = $i18nInstance.language as Language;
    let mycheck: boolean = true;

</script>
<Modal bind:Root={Root}>
    <ScoopedFrame width="[50%]" height="[80%]">
        <div class="w-full h-full flex flex-col items-start gap-y-10 m-2">
            <h1 class="text-3xl font-medium text-darkgray drop-shadow-md">✦ { _i18n.t("settings.title") }</h1>
            <div class="w-full h-full overflow-y-scroll">
                <TabGroup class="flex flex-row gap-x-20 p-2">
                    <TabList class="flex flex-col items-start gap-y-4">
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ _i18n.t("settings.settings_menu.launcher") }</h2></Tab>
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ _i18n.t("settings.settings_menu.game") }</h2></Tab>
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>Tools</h2></Tab>
                    </TabList>
                    <TabPanels class="w-full">
                        <TabPanel class="panel">
                            <ul>
                                <li>
                                    <h3>{ _i18n.t("settings.settings_content.launcher.update.title") }</h3>
                                    <p>{ _i18n.t("settings.settings_content.launcher.update.description", { version: "0.0.0" }) }</p>
                                    <button class="input">{ _i18n.t("settings.settings_content.launcher.update.button_text") }</button>
                                    <div class="inline-flex items-center">
                                        <Checkbox bind:checked={mycheck}/><p>&nbsp;{ _i18n.t("settings.settings_content.launcher.update.search_for_updates_on_start") }</p>
                                    </div>
                                </li>
                                <li>
                                    <h3>{ _i18n.t("settings.settings_content.launcher.language.title") }</h3>
                                    <Listbox value={selectedLanguage} on:change={(e) => { selectedLanguage = e.detail; _i18n.changeLanguage(e.detail) }}>
                                        <ListboxButton class="input listbox-button">
                                            { LanguageName.get(selectedLanguage) }<Icon icon="material-symbols:arrow-drop-down"/>
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
                                    <h3>{ _i18n.t("settings.settings_content.game.integrity.title") }</h3>
                                    <p>{ _i18n.t("settings.settings_content.game.integrity.description") }</p>
                                    <button class="input" on:click={() => {
                                        Root.Close();
                                        LauncherController.GetInstance().RepairGameFiles();
                                    }}>
                                        { _i18n.t("settings.settings_content.game.integrity.button_text") }
                                    </button>
                                </li>
                                <li>
                                    <h3>{ _i18n.t("settings.settings_content.game.installation_directory.title") }</h3>
                                    <input bind:value={settings.game.installation_path} type="text" class="input text"/>
                                </li>
                            </ul>
                        </TabPanel>
                        <TabPanel class="panel">
                            Content 3
                        </TabPanel>
                    </TabPanels>
                </TabGroup>
            </div>
            <div></div>
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

        @apply w-full flex flex-col items-start gap-y-2 text-gray-800

    }

    .panel ul li h3 {

        @apply text-xl drop-shadow-md

    }

    .panel ul li p {

        @apply text-sm text-left

    }

    .panel ul li .input.text {

        @apply w-full

    }

</style>