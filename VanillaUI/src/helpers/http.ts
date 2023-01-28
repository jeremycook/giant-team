import Exception from './Exception';
import { parseJsonResponse } from './httpHelpers';
import { log } from './log';

export namespace http {
    class HttpStatusException extends Exception {
        constructor(
            public source: any,
            public message: string,
            public status?: number,
            public statusText?: string,
            public details?: any
        ) {
            super(source, message, status, statusText, details);
        }
    }

    export const postJson = async <TInput, TData>(url: string, input?: TInput): Promise<TData> => {
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
                    return data;
                }
                else {
                    const message = await response.text();
                    const result: any = {
                        ok: true,
                        status: response.status,
                        message: message ?? '',
                        data: null,
                    };
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

                    // TODO: Throw HttpStatusException that can be caught and handle appropriately.
                    throw new HttpStatusException(http.postJson, errorMessage, response.status, response.statusText, details);
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

                    throw new HttpStatusException(http.postJson, errorMessage, response.status, response.statusText);
                }
            }
        }
        catch (err) {
            const errorMessage = 'Unable to connect to the server. Please check your Internet connection or try again later.';
            const status = undefined;
            const statusText = undefined;
            const details = err;
            throw new HttpStatusException(http.postJson, errorMessage, status, statusText, details);
        }
    }
}
