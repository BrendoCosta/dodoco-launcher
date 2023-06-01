<script lang="ts">
    
    export let _class: string = "";
    export let pill: boolean = false;
    export let additionalClass: string = "";
    export { _class as class };

</script>
<div class="{_class != "" ? _class : `
    w-auto bg-black/20 flex flex-row justify-center items-center
    rounded-2xl [&>*:first-child]:rounded-l-2xl [&>*:last-child]:rounded-r-2xl
    ` }
    { additionalClass }
">
    <slot/>
</div>