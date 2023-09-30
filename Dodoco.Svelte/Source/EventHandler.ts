import { Nullable } from "./";

export class EventHandler<TEventArgs> {
    
    private handlers: Array<(sender: Nullable<object>, args: TEventArgs) => void> = [];

    public Add(handler: (sender: Nullable<object>, args: TEventArgs) => void): void {

        this.handlers.push(handler);

    }

    public Remove(handler: (sender: Nullable<object>, args: TEventArgs) => void): void {

        this.handlers = this.handlers.filter(h => h !== handler);

    }

    public Invoke(sender: Nullable<object>, args: TEventArgs): void {

        this.handlers.slice(0).forEach(h => h(sender, args));

    }

}