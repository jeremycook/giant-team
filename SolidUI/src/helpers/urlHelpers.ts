export const combinePaths = (basePath: string, ...paths: string[]) => {
    return [basePath, ...paths].join('/');
}

export const createUrl = (url: string, params?: Record<string, string>) => {
    if (isLocalUrl(url)) {
        const search = params ? new URLSearchParams(params).toString() : '';
        if (search.length > 0) {
            return encodeURI(url) + '?' + search;
        }
        else {
            return encodeURI(url);
        }
    }
    else {
        throw `The url "${url}" must be relative.`;
    }
}

export function isLocalUrl(url?: string | URL) {
    if (typeof url === 'string') {
        url = new URL(url, location.hostname);
    }
    if (typeof url === 'undefined') {
        return null;
    }
    else {
        return url.hostname === location.hostname;
    }
}
