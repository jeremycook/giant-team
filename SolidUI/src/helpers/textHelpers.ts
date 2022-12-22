export function snakeCase(text: string) {
    return text.replaceAll(/[^\w]+/gi, '_');
}