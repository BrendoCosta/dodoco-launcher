import { IModalControl } from "./IModalControl";

export interface IConfirmPopupControl extends IModalControl {

    Choose(choose: boolean, callback: (result: boolean) => void): void;

}