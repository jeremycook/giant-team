/** Returns the value by either executing the function or returning the value directly. */
export function reveal<T>(value: T | (() => T)) {
    return typeof value === 'function' ? (value as () => T)() : value;
}