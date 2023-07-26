import { EventHandler } from "@Dodoco/index";
import { IConfirmPopupControl } from "./IConfirmPopupControl";
import { get, Readable, Writable, writable } from "svelte/store";
import { TConfirmPopupProperties } from "./TConfirmPopupProperties";
import { ModalControl } from "./ModalControl";

export class ConfirmPopupControl extends ModalControl implements IConfirmPopupControl {

    protected _Properties: Writable<TConfirmPopupProperties> = writable({

        IsOpen: false,
        Closable: false,
        Callback: () => {},

    });

    public override get Properties() { return this._Properties as Readable<TConfirmPopupProperties> }
    public OnChoose: EventHandler<boolean> = new EventHandler<boolean>();

    public constructor(callback: (result: boolean) => void) {
       
        super();
        this._Properties.update(p => { p.Callback = callback; return p; });

    }

    public override Open() {

        super.Open();
        this._Properties.update(p => { p.Closable = false; return p; });

    }

    public Choose(choose: boolean): void {

        this._Properties.update(p => { p.Closable = true; return p; });
        this.Close();
        get(this._Properties).Callback(choose);
        this.OnChoose.Invoke(this, choose);
        return;

    }

}