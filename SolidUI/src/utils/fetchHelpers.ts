export enum HttpStatusCode {
    Ok = 200,
    BadRequest = 400,
    NotFound = 404,
}

export interface DataResponse<T> {
    status: number,
    message?: string,
    data?: T
}

export const postJson = async <TInput, TOutput>(url: string, input: TInput): Promise<DataResponse<TOutput>> => {
    const response = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });

    if (response.ok) {
        const data = await response.json();
        return {
            status: response.status,
            message: response.statusText,
            data: data
        };
    }
    else {
        return {
            status: response.status,
            message: response.statusText,
        }
    }
}