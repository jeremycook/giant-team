import { useNavigate } from '@solidjs/router';
import { ObjectStatus } from '../api/GiantTeam.ComponentModel.Models';
import { authorize, refreshSession } from '../utils/session';

export enum HttpStatusCode {
    ConnectionFailure = -1,
    Ok = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500,
}

export type ObjectStatusResponse = {
    ok: false
} & ObjectStatus

export type OkDataResponse<TData> = {
    ok: true,
    status: number,
    message: string,
    data: TData,
}

export type DataResponse<TData> = OkDataResponse<TData> | ObjectStatusResponse

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
                const result: DataResponse<TData> = {
                    ok: true,
                    status: response.status,
                    message: data?.message ?? '',
                    data: data,
                };
                console.debug(url, result);
                return result;
            }
            else {
                const message = await response.text();
                const result: DataResponse<any> = {
                    ok: true,
                    status: response.status,
                    message: message ?? '',
                    data: null,
                };
                console.debug(url, result);
                return result;
            }
        }
        else {
            if (response.status === 401) {
                // Sync with the server and re-authorize
                await refreshSession();
                authorize();

                const result: ObjectStatusResponse = {
                    ok: false,
                    status: response.status,
                    statusText: 'Login Required',
                    message: 'Please login to access the requested resource.',
                    details: []
                };
                console.debug(url, result);
                return result;
            }
            else if (response.status === 403) {
                const navigate = useNavigate();
                navigate('/access-denied', { state: { returnUrl: location.href } });

                const result: ObjectStatusResponse = {
                    ok: false,
                    status: response.status,
                    statusText: 'Access Denied',
                    message: 'You do not have permission to access the requested resource.',
                    details: []
                };
                console.debug(url, result);
                return result;
            }
            else if (isJsonResponse) {
                const data = await response.json();
                const result: ObjectStatusResponse = {
                    status: response.status,
                    statusText: response.statusText,
                    ...data,
                    ok: false,
                };
                console.debug(url, result);
                return result;
            }
            else {
                const errorMessage = await response.text();
                const result: ObjectStatusResponse = {
                    ok: false,
                    status: response.status,
                    statusText: response.statusText ?? 'Unexpected Error',
                    message: errorMessage ?? 'An unexpected error occurred.',
                    details: [],
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
            statusText: "Network Connection Failure",
            message: 'Unable to connect to the server. Please check your Internet connection or try again later.',
            details: [],
        } as ObjectStatusResponse;
    }
}
