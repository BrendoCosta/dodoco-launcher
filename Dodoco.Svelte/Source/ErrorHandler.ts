import { CommonErrorData } from "./CommonErrorData";
import { _AppError } from "./Global";

export class ErrorHandler {

    public static PushError(err: any) {

        _AppError.update(arr => { arr.push(err.data as CommonErrorData); return arr });

    }

}