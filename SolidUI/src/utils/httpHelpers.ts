import { useNavigate } from '@solidjs/router';
import { authorize } from '../session';

export enum HttpStatusCode {
    ConnectionFailure = -1,
    Ok = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500,
}

export interface DataResponse<TData> {
    ok: boolean,
    status: number,
    message: string,
    data: TData | null,
    errorData?: any,
}

export const postJson = async <TInput, TData>(url: string, input?: TInput): Promise<DataResponse<TData>> => {
    try {

        const body = input ? JSON.stringify(input) : null;
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: body
        });

        const isJsonResponse = response.headers.get('Content-Type')?.startsWith('application/json') === true;

        if (response.ok) {
            if (isJsonResponse) {
                const data = await response.json();
                const result = {
                    ok: true,
                    status: response.status,
                    message: data?.message ?? '',
                    data: data,
                    errorData: null,
                };
                console.debug(url, result);
                return result;
            }
            else {
                const message = await response.text();
                const result = {
                    ok: true,
                    status: response.status,
                    message: message ?? '',
                    data: null,
                    errorData: null,
                };
                console.debug(url, result);
                return result;
            }
        }
        else {
            if (response.status === 401) {
                await authorize();
                throw 'Unreachable reached!';
            }
            else if (response.status === 403) {
                const navigate = useNavigate();
                navigate('/access-denied', { state: { returnUrl: location.href } });
                throw 'Unreachable reached!';
            }
            else if (isJsonResponse) {
                const data = await response.json();
                const result = {
                    ok: false,
                    status: response.status,
                    message: data?.message ?? '',
                    data: null,
                    errorData: data,
                };
                console.debug(url, result);
                return result;
            }
            else {
                const errorMessage = await response.text();
                const result = {
                    ok: false,
                    status: response.status,
                    message: errorMessage ?? '',
                    data: null,
                    errorData: null,
                };
                console.debug(url, result);
                return result;
            }
        }
    }
    catch (err) {
        // TODO: Upload errors
        console.error(url, err);
        return {
            ok: false,
            status: HttpStatusCode.ConnectionFailure,
            message: 'Unable to connect to server. Please check your Internet connection or try again later.',
            data: null,
            errorData: err,
        }
    }
}
