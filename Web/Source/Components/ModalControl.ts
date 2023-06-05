import { IModalControl } from "./IModalControl";
import { get, Readable, Writable, writable } from "svelte/store";
import { TModalProperties } from "./TModalProperties";

export class ModalControl implements IModalControl {

    protected _Properties: Writable<TModalProperties> = writable({

        IsOpen: false

    });

    public get Properties() { return this._Properties as Readable<TModalProperties> }
    
    public Open(): void {

        if (get(this._Properties).IsOpen) return;
        this._Properties.update(p => { p.IsOpen = true; return p; });

    }
    
    public Close(): void {

        if (!get(this._Properties).IsOpen) return;
        this._Properties.update(p => { p.IsOpen = false; return p; });

    }

}