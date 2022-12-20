export function debug<T>(data: T, label?: string) {
    if (label) {
        console.debug(label, data);
    }
    else {
        console.debug(data);
    }
    return data;
}

export function log<T>(data: T, label?: string) {
    if (label) {
        console.log(label, data);
    }
    else {
        console.log(data);
    }
    return data;
}