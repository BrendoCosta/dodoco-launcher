<script lang="ts">
    
	import { GlobalInstances, Nullable } from "@Dodoco/index";
	import { onMount } from "svelte";
	import Router, { push } from "svelte-spa-router";
	// Generated types
	import { LauncherActivityState } from "@Dodoco/Generated/Dodoco/Launcher/LauncherActivityState";
    import { GameState } from "@Dodoco/Generated/Dodoco/Game/GameState";
	
	export let routes: any;

	onMount(async () => {

		push("/Splash");

		GlobalInstances.LauncherInstance.subscribe((instance) => {

			if (instance.Game != null) {

				if (
					instance.Game.State == GameState.WAITING_FOR_DOWNLOAD ||
					instance.Game.State == GameState.WAITING_FOR_UPDATE ||
					instance.Game.State == GameState.READY
				) {
					
					push("/Launcher");

				}

			}

		});

	});

</script>
<svelte:head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width,initial-scale=1">
	<link rel="icon" type="image/png" href="/favicon.png">
	<link rel="stylesheet" href="/Stylesheet/Style.css">
	<title>Svelte app</title>
</svelte:head>
<div id="main-wrapper" class="w-full h-full">
	<Router {routes}/>
</div>