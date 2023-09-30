import { TModalProperties } from "./TModalProperties";

export interface TConfirmPopupProperties extends TModalProperties {

    Callback: (result: boolean) => void

}