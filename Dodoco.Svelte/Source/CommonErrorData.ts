export interface CommonErrorData {

	type?: string;
	message?: string;
	stack?: string;
	code: number;
	inner?: CommonErrorData;
	
}
