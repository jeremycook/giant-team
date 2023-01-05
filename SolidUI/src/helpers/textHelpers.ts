export function snakeCase(text: string) {
    return text.replaceAll(/[^\w]+/gi, '_');
}

export function camelCase(text: string) {
    return text
        .replace(/^[A-Z]/, ch => ch.toLowerCase())
        .replaceAll(/_([a-z])/g, (_, ch) => ch.toUpperCase());
}