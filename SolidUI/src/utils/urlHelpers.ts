export const createUrl = (url: string, params?: Record<string, string>) => {
    const isRelative = !/^([\sA-Z]*:|\s*\/\/)/i.test(url);
    if (isRelative) {
        const search = params ?  new URLSearchParams(params).toString() : '';
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