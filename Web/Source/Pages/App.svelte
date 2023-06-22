<script lang="ts">
	
    import { GameState } from "@Dodoco/Generated/Dodoco/Core/Game/GameState";
    
	import { _MainViewData } from "@Dodoco/Global";
	import { onMount } from "svelte";
	import Router, { push } from "svelte-spa-router";
	// Generated types
	
	export let routes: any;

	onMount(async () => {

		push("/Splash");

		_MainViewData.subscribe((data) => {

			if (
				data.Game?.State == GameState.WAITING_FOR_DOWNLOAD ||
				data.Game?.State == GameState.WAITING_FOR_UPDATE ||
				data.Game?.State == GameState.READY
			) {
				
				push("/Main");

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