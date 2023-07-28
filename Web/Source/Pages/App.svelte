<script lang="ts">
	
    import { Popup } from "@Dodoco/Components";
	import { _AppError, _LauncherState, i18nInstance } from "@Dodoco/Global";
	import { onMount } from "svelte";
	import Router, { push } from "svelte-spa-router";
	// Generated types
	import { LauncherState } from "@Dodoco/Generated/Dodoco/Core/Launcher/LauncherState";
	
	export let routes: any;

	onMount(async () => {

		push("/Splash");

		_LauncherState.subscribe((data) => {

			if ($_LauncherState == LauncherState.READY) {
				
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
	{#each $_AppError as err }
		<Popup open={true} title={`${$i18nInstance.t("component.popup.error_title")} ${err.code ?? ""}`} callback={() => {

			_AppError.update(arr => { arr.pop(); return arr });

		} }>
			<p>
				{#if err.type}
					<strong>Type:</strong><br>
					{err.type}<br>
				{/if}
				{#if err.message}
					<strong>Message:</strong><br>
					{err.message}<br>
				{/if}
				{#if err.stack}
					<strong>Stack:</strong><br>
					{err.stack}<br>
				{/if}
				{#if err.inner}
					<strong>Inner exception:</strong>
					{JSON.stringify(err.inner)}
				{/if}
			</p>
		</Popup>
	{/each}
</div>