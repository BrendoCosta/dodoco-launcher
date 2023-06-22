import { Nullable } from "@Dodoco/index";
import { TRadioItem } from "./TRadioItem";

export class RadioGroup<T> {

    public Items: TRadioItem<T>[] = [];

    public constructor(itemsArray: T[])
    public constructor(itemsArray: T[], radioItems?: TRadioItem<T>[]) {

        if (radioItems) {

            radioItems.map(i => {
            
                if (!i.Selected) {
    
                    i.Selected = false;
    
                }
    
            });
    
            this.Items = radioItems;

        } else {

            let itemsAux: TRadioItem<T>[] = [];
            itemsArray.forEach(i => itemsAux.push({ Value: i, Selected: false }));
            this.Items = itemsAux;

        }

    }

    public set Selected(value: T) {

        this.Items.map(i => (i.Selected = false));

        if (this.Items.find(i => i.Value = value) != null) {

            this.Items.find(i => i.Value = value)!.Selected = true; 

        }

    }

    public get Selected(): Nullable<T> {

        return this.Items.find(i => i.Selected)?.Value;

    }

}