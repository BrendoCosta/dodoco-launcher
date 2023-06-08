<svelte:options accessors={true}></svelte:options>
<script lang="ts">

    import { i18nInstance } from "@Dodoco/GlobalInstances";
    import { Checkbox, ConfirmPopupControl, Modal, ModalControl, ScoopedFrame } from "@Dodoco/Components";
    import { Language, LanguageConstants, LanguageName } from "@Dodoco/Language";
    
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    import Icon from "@iconify/svelte";

    // Generated types
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Network/Controller/LauncherController";
    import { LauncherSettings } from "@Dodoco/Generated/Dodoco/Launcher/Settings/LauncherSettings";
    import ConfirmPopup from "@Dodoco/Components/ConfirmPopup.svelte";
    import { onMount } from "svelte";
    import { Writable, writable } from "svelte/store";

    export let Root: ModalControl = new ModalControl();

    let confirmRestoreSettings: ConfirmPopupControl;
    let confirmSaveSettings: ConfirmPopupControl;

    let EditableSettings: Writable<LauncherSettings> = writable({} as LauncherSettings);

    onMount(async () => {

        // Load the current settings whenever modal opens

        Root.OnOpen.Add(async (sender, args) => {

            EditableSettings = writable(await LauncherController.GetInstance().GetLauncherSettings());

        });

    });

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
                        <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>Tools</h2></Tab>
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
                                        <Checkbox bind:checked={$EditableSettings.launcher.auto_search_for_updates}/><p>&nbsp;{ $i18nInstance.t("settings.content.launcher.update.auto_search_for_updates") }</p>
                                    </div>
                                </li>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.launcher.language.title") }</h3>
                                    <Listbox value={$EditableSettings.launcher.language} on:change={(e) => { $EditableSettings.launcher.language = e.detail; }}>
                                        <ListboxButton class="input listbox-button">
                                            { LanguageName.get($EditableSettings.launcher.language) }<Icon icon="material-symbols:arrow-drop-down"/>
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
                                        LauncherController.GetInstance().RepairGameFiles();
                                    }}>
                                        { $i18nInstance.t("settings.content.game.integrity.button_text") }
                                    </button>
                                </li>
                                <li>
                                    <h3>{ $i18nInstance.t("settings.content.game.installation_directory.title") }</h3>
                                    <input bind:value={$EditableSettings.game.installation_directory} type="text" class="input text"/>
                                </li>
                            </ul>
                        </TabPanel>
                        <TabPanel class="panel">
                            Content 3
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
        await LauncherController.GetInstance().SetLauncherSettings($EditableSettings);
        
        // Changes the UI language
        $i18nInstance.changeLanguage($EditableSettings.launcher.language);

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