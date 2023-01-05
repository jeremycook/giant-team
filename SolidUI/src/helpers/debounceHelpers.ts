export function debounce<T extends Function>(callback: T, wait = 300) {
    throw 'Untested';
    let timeout = 0;
    const callable = (...args: any) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => callback(...args), wait);
    };
    return callable as unknown as T;
}