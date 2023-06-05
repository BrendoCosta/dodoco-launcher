import { Readable } from "svelte/store";

export interface IComponentControl<T> {

    Properties: Readable<T>;

}