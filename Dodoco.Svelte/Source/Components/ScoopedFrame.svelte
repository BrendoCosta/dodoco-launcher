<script lang="ts">

    export let width: string = "1/2";
    export let height: string = "1/2";

</script>
<div class="relative h-{height} w-{width} bg-white p-1 flex items-center justify-center shadow-md">
    <div class="w-full h-full p-0.5 flex items-center justify-center bg-zinc-200/70 dodoco-scooped">
        <div class="w-full h-full p-6 bg-white flex dodoco-scooped">
            <slot/>
        </div>
    </div>
</div>
<style>

    .dodoco-scooped {

        mask: radial-gradient(1.5rem at 1.5rem 1.5rem,#0000 95%,#000) -1.5rem -1.5rem;

    }

</style>