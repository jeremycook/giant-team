export const combinePaths = (basePath: string, ...paths: string[]) => {
    return [basePath, ...paths].join('/');
}

export const createHref = (href: string, params?: { [key: string]: string }) => {
    return createUrl(href, params).toString();
}

export const createUrl = (href: string, params?: { [key: string]: string }) => {
    const url = new URL(href + (params && Object.keys(params).length > 0 ? '?' + new URLSearchParams(params).toString() : ''), location.href);
    if (isLocalUrl(url)) {
        return url;
    }
    else {
        throw `The url "${href}" must be relative.`;
    }
}

export const relativeHref = (url: URL) => {
    return url.pathname + url.search + url.hash;
}

export function isLocalUrl(url?: URL | string | null): boolean {
    if (url instanceof URL)
        return url.host === location.host;
    else if (typeof url === 'string') {
        return isLocalUrl(new URL(url, location.href));
    }
    else
        return false;
}
