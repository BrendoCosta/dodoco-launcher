<script lang="ts">

    import { GameInstance, LauncherInstance, i18nInstance } from "@Dodoco/GlobalInstances";
    import { Language, LanguageConstants, LanguageName } from "@Dodoco/Language";
    import { Button, ButtonGroup, Checkbox, LoadingDots, Modal, ModalControl, ProgressBar, ScoopedFrame } from "@Dodoco/Components";
    import Settings from "./Settings.svelte";

    import Icon from "@iconify/svelte";
    import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@rgossiaux/svelte-headlessui";
    import { Listbox, ListboxButton, ListboxOptions, ListboxOption } from "@rgossiaux/svelte-headlessui";
    
    // Generated types
    import { LauncherController } from "@Dodoco/Generated/Dodoco/Network/Controller/LauncherController";
    import { GameState } from "@Dodoco/Generated/Dodoco/Game/GameState";

    $: game = $GameInstance;
    $: _i18n = $i18nInstance;
    let settingsModal: ModalControl;
    let progressBarText: string = "";
    let progressBarValue: number = 0;
    let mycheck: boolean = true;

    let selectedLanguage = $i18nInstance.language as Language;

    $: IsBusy = (): boolean => {
        return game.State == GameState.CHECKING_INTEGRITY
            || game.State == GameState.DOWNLOADING
            || game.State == GameState.RUNNING
            || game.State == GameState.UPDATING;
    }

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
                                Checking game's files integrity
                            {/if}
                        </small>
                    {/if}
                    <ProgressBar show={IsBusy()} value={0} width="1/4"/>
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
    {#await LauncherController.GetInstance().GetLauncherSettings() then settings }
        <Settings bind:Root={settingsModal} settings={settings}/>
    {/await}
</div>
<style>
</style>