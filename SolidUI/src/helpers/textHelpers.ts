export function snakeCase(text: string) {
    return text.replaceAll(/[^\w]+/gi, '_');
}

export function camelCase(text: string) {
    return text
        .replace(/^./, ch => ch.toLowerCase())
        .replaceAll(/_(.)/g, (_, ch) => ch.toUpperCase());
}