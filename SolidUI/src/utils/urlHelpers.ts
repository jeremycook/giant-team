export const createUrl = (url: string, params?: Record<string, string>) => {
    if (params) {
        const search = new URLSearchParams(params);
        return new URL(url + "?" + search, location.href);
    }
    else {
        return new URL(url, location.href);
    }
}