import { IModalControl } from "./IModalControl";

export interface IConfirmPopupControl extends IModalControl {

    Confirm(choose: boolean): void;

}