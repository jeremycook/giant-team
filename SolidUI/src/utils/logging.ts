export function debug<T>(data: T, label?: string) {
    if (label) {
        console.debug(label, data);
    }
    else {
        console.debug(data);
    }
    return data;
}