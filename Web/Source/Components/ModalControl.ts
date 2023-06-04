import { IModalControl } from "./IModalControl";

export class ModalControl implements IModalControl {

    private isOpen: boolean = false;
    public get IsOpen() { return this.isOpen }
    
    public Open(): void {

        if (this.isOpen) return;
        this.isOpen = true;

    }
    
    public Close(): void {

        if (!this.isOpen) return;
        this.isOpen = false;

    }

}