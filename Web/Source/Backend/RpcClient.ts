import { Nullable } from "@Dodoco/index";
import { Backend } from ".";

import { RequestManager, Client, WebSocketTransport } from "@open-rpc/client-js";
import urlJoin from "url-join";

type Constructor<T> = new (...args: any[]) => T;

function ofType<TElements, TFilter extends TElements>(array: TElements[], filterType: Constructor<TFilter>): TFilter[] {
    return <TFilter[]>array.filter(e => e instanceof filterType);
}

export class RpcClient {

    private static instance: Nullable<RpcClient> = null;

    private transport: WebSocketTransport;
    private requestManager: RequestManager;
    private client: Client;
    private connected: boolean = false;

    protected constructor() {

        this.transport = new WebSocketTransport(Backend.RpcUrl);
        this.transport.connection.onopen = () => this.connected = true;
        this.transport.connection.onclose = () => this.connected = false;
        //this.transport.connection.onmessage = (data: any) => console.debug(data);
        this.requestManager = new RequestManager([this.transport]);
        this.client = new Client(this.requestManager);
        this.client.onError((e) => console.log(e));

    }

    public async Call<T>(method: string, params?: any[]): Promise<Nullable<T>> {

        if (!this.connected) return null;

        let result: Nullable<T> = null;

        try {

            result = await this.client.request({ method: method, params: params }) as T;

        } catch (e: any) {}

        return result;

    }

    public static GetInstance(): RpcClient {

        if (RpcClient.instance == null) {

            RpcClient.instance = new RpcClient();

        }

        return RpcClient.instance;

    }

}