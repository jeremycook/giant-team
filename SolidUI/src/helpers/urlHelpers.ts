export const combinePaths = (basePath: string, ...paths: string[]) => {
    return [basePath, ...paths].join('/');
}

export const createHref = (href: string, params?: { [key: string]: string }) => {
    return createUrl(href).toString();
}

export const createUrl = (href: string, params?: { [key: string]: string }) => {
    const url = new URL(href + params ? '?' + new URLSearchParams(params).toString() : '', location.href);
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

export function isLocalUrl(url?: string | URL | null) {
    if (typeof url === 'string') {
        const asUrl = new URL(url, location.href);
        return asUrl.hostname === location.hostname;
    }
    else if (typeof url === 'undefined')
        return false;
    else if (url === null)
        return false;
    else
        return url.hostname === location.hostname;
}
