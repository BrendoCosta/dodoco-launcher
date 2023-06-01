export class Backend {

    public static get url(): string {

        return `http://localhost:4000/Dodoco/Network/Controller`;

    }

    public static get RpcUrl(): string {

        return "ws://localhost:4000/Dodoco/Network/Controller/RpcController";

    }

}