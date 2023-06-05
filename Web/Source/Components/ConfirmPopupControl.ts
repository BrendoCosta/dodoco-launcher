import { IConfirmPopupControl } from "./IConfirmPopupControl";
import { IModalControl } from "./IModalControl";
import { get, Readable, Writable, writable } from "svelte/store";
import { TModalProperties } from "./TModalProperties";
import { TConfirmPopupProperties } from "./TConfirmPopupProperties";
import { ModalControl } from "./ModalControl";

export class ConfirmPopupControl extends ModalControl implements IConfirmPopupControl {

    protected _Properties: Writable<TConfirmPopupProperties> = writable({

        IsOpen: false,
        Choose: false,

    });

    public Confirm(choose: boolean): void {
        
        this._Properties.update(p => { p.Choose = choose; return p; });
        this.Close();

    }

}