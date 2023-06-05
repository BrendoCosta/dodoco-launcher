import { IComponentControl } from "./IComponentControl";
import { TModalProperties } from "./TModalProperties";

export interface IModalControl extends IComponentControl<TModalProperties> {

    Open(): void;
    Close(): void;

}