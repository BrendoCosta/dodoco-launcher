import { EventHandler } from "@Dodoco/index";
import { IModalControl } from "./IModalControl";
import { get, Readable, Writable, writable } from "svelte/store";
import { TModalProperties } from "./TModalProperties";

export class ModalControl implements IModalControl {

    protected _Properties: Writable<TModalProperties> = writable({

        IsOpen: false,
        Closable: true,

    });

    public get Properties() { return this._Properties as Readable<TModalProperties> }

    public OnOpen: EventHandler<void> = new EventHandler<void>();
    public OnClose: EventHandler<void> = new EventHandler<void>();
    
    public Open(): void {

        if (get(this._Properties).IsOpen) return;
        this._Properties.update(p => { p.IsOpen = true; return p; });
        this.OnOpen.Invoke(this);

    }
    
    public Close(): void {

        if (!get(this._Properties).IsOpen) return;
        if (!get(this._Properties).Closable) return;
        this._Properties.update(p => { p.IsOpen = false; return p; });
        this.OnClose.Invoke(this);

    }

}