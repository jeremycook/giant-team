import { createUniqueId } from 'solid-js';

/** Calculates a position that is within the viewport. */
export const constrainToViewport = ({ left, top, width, height }: { left: number; top: number; width: number; height: number; }) => {

    const minLeft = visualViewport?.pageLeft ?? window.scrollX;
    const viewportRight = minLeft + (visualViewport?.width ?? window.innerWidth);
    const maxLeft = Math.max(minLeft, viewportRight - width);

    const constrainedLeft = Math.max(minLeft, Math.min(left, maxLeft));

    const minTop = visualViewport?.pageTop ?? window.scrollY;
    const viewportBottom = minTop + (visualViewport?.height ?? window.innerHeight);
    const maxTop = Math.max(minTop, viewportBottom - height);

    const constrainedTop = Math.max(minTop, Math.min(top, maxTop));

    const constrainedPosition = {
        left: constrainedLeft,
        top: constrainedTop,
    };

    return constrainedPosition
}

/** Return a string with a random but fixed prefix followed by the provided suffix. */
export const createId = ((prefix) => (suffix: string) => prefix ? prefix + '_' + suffix : suffix)(createUniqueId());

/** Convert a file to a base64 encoded string */
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

/** Get and elements position, compensating for offset parents. */
export function getElementPosition(element: HTMLElement) {
    let left = 0;
    let top = 0;

    while (typeof element?.offsetLeft === 'number') {
        left += (element.offsetLeft + element.clientLeft - element.scrollLeft);
        top += (element.offsetTop + element.clientTop - element.scrollTop);

        element = element.offsetParent as HTMLElement;
    }

    return {
        left: left,
        top: top
    };
}

export function getScreenCenter() {
    return {
        x: window.innerWidth / 2,
        y: window.innerHeight / 2
    };
}