import { useNavigate } from '@solidjs/router';
import { ObjectStatus } from '../bindings/GiantTeam.ComponentModel.Models';
import { toast } from '../partials/Toasts';
import { log } from './logging';
import { parseJson } from './objectHelpers';

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

/** Parse {@param response} content using {@link parseJson}. */
export const parseJsonResponse = async (response: Response) => {
    const json = await response.text();
    const result = parseJson(json);
    return result;
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
                const data = await parseJsonResponse(response);
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
            if (isJsonResponse) {
                const details = await parseJsonResponse(response);

                let errorMessage = details.message;
                if (response.status === 401) {
                    errorMessage ||= 'You must be logged in to access the requested resource.';
                    // TODO: user.requestLogin();
                }
                else if (response.status === 403) {
                    errorMessage ||= 'You do not have permission to access the requested resource.'
                }
                else {
                    errorMessage ||= 'An unexpected error occurred.'
                    log.warn('Non-OK response in {MemberName}: {ErrorMessage}.', ['postJson', errorMessage]);
                }

                toast.error(errorMessage);

                const result: ObjectStatusResponse = {
                    status: response.status,
                    statusText: response.statusText,
                    message: errorMessage,
                    details: details,
                    ok: false,
                };
                console.debug(url, result);
                return result;
            }
            else {
                let errorMessage = await response.text();
                if (response.status === 401) {
                    errorMessage ||= 'You must be logged in to access the requested resource.'
                    // TODO: user.requestLogin();
                }
                else if (response.status === 403) {
                    errorMessage ||= 'You do not have permission to access the requested resource.'
                }
                else {
                    errorMessage ||= 'An unexpected error occurred.'
                    log.warn('Non-OK response in {MemberName}: {ErrorMessage}.', ['postJson', errorMessage]);
                }

                toast.error(errorMessage);

                const result: ObjectStatusResponse = {
                    ok: false,
                    status: response.status,
                    statusText: response.statusText || 'Unexpected Error',
                    message: errorMessage,
                    details: [],
                };
                console.debug(url, result);
                return result;
            }
        }
    }
    catch (err) {
        log.warn('Suppressed {Error} in {MemberName}.', [err, 'postJson']);
        return {
            ok: false,
            status: HttpStatusCode.ConnectionFailure,
            statusText: "Network Connection Failure",
            message: 'Unable to connect to the server. Please check your Internet connection or try again later.',
            details: [],
        } as ObjectStatusResponse;
    }
}

export function useGo() {
    const navigate = useNavigate();
    return (to: string, state?: { [key: string]: any }) => {
        navigate(to, { state });
    };
}
