import { createUniqueId } from 'solid-js';

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
export function getElementPosition(element: HTMLElement) {
    var xPosition = 0;
    var yPosition = 0;

    // console.log('start', element);
    // while (element?.namespaceURI === 'http://www.w3.org/2000/svg') {
    //     element = (element.parentNode || element.host as HTMLElement;
    // }
    // console.log('begin', element);

    while (typeof element?.offsetLeft === 'number') {

        xPosition += (element.offsetLeft + element.clientLeft - element.scrollLeft);
        yPosition += (element.offsetTop + element.clientTop - element.scrollTop);

        element = element.offsetParent as HTMLElement;
    }
    return {
        x: xPosition,
        y: yPosition
    };
}