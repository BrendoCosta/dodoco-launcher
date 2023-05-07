<script lang="ts">
	
	export let routes: any;

	import { onMount } from "svelte";
	import Router, { push } from "svelte-spa-router";

	onMount(async () => {

		push("/splash");

	});

</script>
<svelte:head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width,initial-scale=1">
	<link rel="icon" type="image/png" href="/favicon.png">
	<link rel="stylesheet" href="/Stylesheet/Style.css">
	<title>Svelte app</title>
</svelte:head>
<div id="app-wrapper">
	<Router {routes}/>
</div>