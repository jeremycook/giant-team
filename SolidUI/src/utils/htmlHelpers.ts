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

// helper function to get an element's exact position
export function getClickPosition(el: HTMLElement) {
    var xPosition = 0;
    var yPosition = 0;

    while (typeof el?.offsetLeft === 'number') {
        if (el.tagName == "BODY") {
            // deal with browser quirks with body/window/document and page scroll
            var xScrollPos = el.scrollLeft || document.documentElement.scrollLeft;
            var yScrollPos = el.scrollTop || document.documentElement.scrollTop;

            xPosition += (el.offsetLeft - xScrollPos + el.clientLeft);
            yPosition += (el.offsetTop - yScrollPos + el.clientTop);
        } else {
            xPosition += (el.offsetLeft - el.scrollLeft + el.clientLeft);
            yPosition += (el.offsetTop - el.scrollTop + el.clientTop);
        }

        el = el.offsetParent as HTMLElement;
    }
    return {
        x: xPosition,
        y: yPosition
    };
}