<script lang="ts">
    
    export let _class: string = "";
    export let pill: boolean = false;
    export let additionalClass: string = "";
    export { _class as class };

</script>
<div class="{_class != "" ? _class : `
    w-auto flex flex-row justify-center items-center
    rounded-md [&>*:first-child]:rounded-l-md [&>*:last-child]:rounded-r-md
    ` }
    { additionalClass }
">
    <slot/>
</div>