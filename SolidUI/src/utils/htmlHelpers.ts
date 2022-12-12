import { createUniqueId } from 'solid-js';

/** Return a string with a random but fixed prefix followed by the provided suffix. */
export const createId = ((prefix) => (suffix: string) => prefix ? prefix + '_' + suffix : suffix)(createUniqueId());

/** Convert a file to an base64 encoded string */
export const stringifyBlob = async (file: Blob): Promise<string> => {
    return await new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => {
            const result = reader.result as string;
            const start = result.indexOf(',') + 1;
            resolve(result.slice(start));
        };
        reader.onerror = error => reject(error);
    });
}
