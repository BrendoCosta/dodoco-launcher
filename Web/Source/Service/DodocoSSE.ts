import { Dodoco } from "../Api";

let DodocoSSE: EventSource = new EventSource(`${Dodoco.url}/SSE`);

export { DodocoSSE }