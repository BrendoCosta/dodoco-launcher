<script lang="ts">
    
	import { GlobalInstances } from "@Dodoco/index";
	import { onMount } from "svelte";
	import Router, { push } from "svelte-spa-router";
	// Generated types
    import { GameState } from "@Dodoco/Generated/Dodoco/Game/GameState";
	
	export let routes: any;

	onMount(async () => {

		push("/Splash");

		GlobalInstances.GameInstance.subscribe((instance) => {

			if (
				instance.State == GameState.WAITING_FOR_DOWNLOAD ||
				instance.State == GameState.WAITING_FOR_UPDATE ||
				instance.State == GameState.READY
			) {
				
				push("/Launcher");

			}

		});

	});

</script>
<svelte:head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width,initial-scale=1">
	<link rel="icon" type="image/png" href="/favicon.png">
	<link rel="stylesheet" href="/Stylesheet/Style.css">
	<title>Dodoco Launcher</title>
</svelte:head>
<div id="main-wrapper" class="w-full h-full">
	<Router {routes}/>
</div>