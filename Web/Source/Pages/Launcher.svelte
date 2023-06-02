<script lang="ts">

    import { GlobalInstances, Nullable } from "@Dodoco/index";
    import i18next, { i18n, t } from "i18next";
    import { LanguageName } from "@Dodoco/Language"
    // Generated types
    import { LauncherActivityState } from "@Dodoco/Generated/Dodoco/Launcher/LauncherActivityState";
    import { onMount } from "svelte";
    import { get } from "svelte/store";
    import Icon from "@iconify/svelte";
    import { Button, ButtonGroup, Checkbox, LoadingDots, Modal, ProgressBar, ScoopedFrame } from "@Dodoco/Components";
        import {
        Tab,
        TabGroup,
        TabList,
        TabPanel,
        TabPanels,
    } from "@rgossiaux/svelte-headlessui";
    import {
        Listbox,
        ListboxButton,
        ListboxOptions,
        ListboxOption,
    } from "@rgossiaux/svelte-headlessui";
    import { GameState } from "@Dodoco/Generated/Dodoco/Game/GameState";
    import { RpcClient } from "@Dodoco/Backend";
    import { IGame } from "@Dodoco/Generated/Dodoco/Game/IGame";
    import { Language, LanguageConstants } from "@Dodoco/Language";
    import { Translator } from "@Dodoco/Language/Translator";

    let game = {
        State: GameState.NOT_INSTALLED
    } as IGame;
    let settingsModal: Modal;
    let progressBarText: string = "";
    let progressBarValue: number = 0;
    let mycheck: boolean = true;

    const people = [
        { id: 1, name: "Durward Reynolds", unavailable: false },
        { id: 2, name: "Kenton Towne", unavailable: false },
        { id: 3, name: "Therese Wunsch", unavailable: false },
        { id: 4, name: "Benedict Kessler", unavailable: true },
        { id: 5, name: "Katelyn Rohan", unavailable: false },
    ];

    let selectedLanguage = Language.en_US;
    let translation: i18n = i18next.cloneInstance();

    setInterval(async () => {

        let result: Nullable<IGame> = await RpcClient.GetInstance().Call("Dodoco.Network.Controller.GlobalInstancesController.GetGameInstance");

        if (result != null) {

            game = result;

        }

        translation = i18next;

    }, 100);

    $: IsBusy = (): boolean => {
        return game.State == GameState.CHECKING_INTEGRITY
            || game.State == GameState.DOWNLOADING
            || game.State == GameState.RUNNING
            || game.State == GameState.UPDATING;
    }

    let selectedTableHelper = (a: any) => a.selected ? "my-custom-class active" : "my-custom-class";

</script>
<div id="launcher-wrapper">
    <div class="w-full h-full bg-gradient-to-t from-black/80 to-black/0">
        <div class="w-full h-full w-full flex flex-col items-center p-4">
            <div class="w-full h-full flex flex-col">
                <div class="w-full h-full flex flex-col items-center justify-start">
                    
                </div>
                <div class="w-full h-full">

                </div>
                <div class="w-full h-full flex flex-col items-center justify-center gap-y-3">
                    {#if IsBusy() }
                        <small class="text-white animate-pulse">
                            {#if game?.State == GameState.CHECKING_INTEGRITY }
                                Checking game's files integrity ({game?.CheckIntegrityProgressReport.completionPercentage.toFixed(1)}%) / ETA: {game?.CheckIntegrityProgressReport.estimatedRemainingTime}
                            {/if}
                        </small>
                    {/if}
                    <ProgressBar show={IsBusy()} value={ game?.State == GameState.CHECKING_INTEGRITY ? game?.CheckIntegrityProgressReport.completionPercentage : 0} width="1/4"/>
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
    <div class="relative z-10">
        <Modal show bind:this={settingsModal}>
            <ScoopedFrame width="[50%]" height="[80%]">
                <div class="w-full h-full flex flex-col items-start gap-y-10 m-2">
                    <h1 class="text-3xl font-medium text-darkgray drop-shadow-md">✦ { translation.t("settings.title") }</h1>
                    <div class="w-full h-full overflow-y-scroll">
                        <TabGroup class="flex flex-row gap-x-20">
                            <TabList class="flex flex-col items-start gap-y-4">
                                <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ translation.t("settings.settings_menu.launcher") }</h2></Tab>
                                <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>{ translation.t("settings.settings_menu.game") }</h2></Tab>
                                <Tab class={(e) => e.selected ? "[&>h2]:(font-medium text-gray-800 drop-shadow-md)" : "[&>h2]:(text-gray-800/50 drop-shadow-md)"}><h2>Tools</h2></Tab>
                            </TabList>
                            <TabPanels>
                                <TabPanel class="flex flex-col justify-start items-start">
                                    <ul class="flex flex-col items-start gap-y-6 [&>li]:(flex flex-col items-start gap-y-2) [&>li>h3]:(text-xl drop-shadow-md) [&_p]:(text-sm text-left text-gray-800)">
                                        <li>
                                            <h3>{ translation.t("settings.settings_content.launcher.update.title") }</h3>
                                            <p>{ translation.t("settings.settings_content.launcher.update.description", { version: "0.0.0" }) }</p>
                                            <button class="transition-all ease-in-out border-1 border-gray-300 text-gray-800 py-2 px-4 rounded-xl text-sm hover:(bg-gray-200/80)">{ translation.t("settings.settings_content.launcher.update.button_text") }</button>
                                            <div class="inline-flex items-center">
                                                <Checkbox bind:checked={mycheck}/><p>&nbsp;{ translation.t("settings.settings_content.launcher.update.search_for_updates_on_start") }</p>
                                            </div>
                                        </li>
                                        <li>
                                            <h3>{ translation.t("settings.settings_content.launcher.language.title") }</h3>
                                            <Listbox value={selectedLanguage} on:change={(e) => { selectedLanguage = e.detail; i18next.changeLanguage(e.detail) }}>
                                                <ListboxButton class="input w-64 flex flex-row items-center justify-between py-2 px-4 gap-x-3 text-sm text-left">{LanguageName.get(selectedLanguage)}<Icon icon="material-symbols:arrow-drop-down"/></ListboxButton>
                                                <ListboxOptions class="w-64 flex flex-col mt-1 border-1 border-gray-300 bg-white text-sm text-left rounded-xl [&>:first-child]:rounded-t-xl [&>:last-child]:rounded-b-xl cursor-pointer">
                                                    {#each LanguageConstants.SupportedLanguages as lang }
                                                    <ListboxOption class="py-3 px-4 text-gray-800 hover:bg-gray-200/80" value={lang}>
                                                        { LanguageName.get(lang) }
                                                    </ListboxOption>
                                                    {/each}
                                                </ListboxOptions>
                                            </Listbox>                                              
                                        </li>
                                    </ul>
                                </TabPanel>
                                <TabPanel class="flex flex-col justify-start items-start">
                                    <ul class="flex flex-col items-start gap-y-6 [&>li]:(flex flex-col items-start gap-y-2) [&>li]:(flex flex-col items-start gap-y-2) [&>li>h3]:(text-xl drop-shadow-md) [&>li>p]:(text-sm text-left)">
                                        <li>
                                            <h3>{ translation.t("settings.settings_content.game.integrity.title") }</h3>
                                            <p>{ translation.t("settings.settings_content.game.integrity.description") }</p>
                                            <button class="transition-all ease-in-out border-1 border-gray-300 text-gray-800 py-2 px-4 rounded-xl text-sm hover:(bg-gray-200/80)" on:click={() => { settingsModal.Close(); RpcClient.GetInstance().Call("Dodoco.Launcher.ILauncher.RepairGameFiles", []); }}>
                                                { translation.t("settings.settings_content.game.integrity.button_text") }
                                            </button>
                                        </li>
                                    </ul>
                                </TabPanel>
                                <TabPanel>Content 3</TabPanel>
                            </TabPanels>
                        </TabGroup>
                    </div>
                    <div></div>
                </div>
            </ScoopedFrame>
        </Modal>
	</div>
</div>
<style>
</style>