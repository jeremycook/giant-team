import { createRenderEffect } from "solid-js";

declare module "solid-js" {
    namespace JSX {
        interface Directives {
            autofocus: true;
        }
    }
}

export const autofocus = (inputElement: HTMLInputElement) => {

    createRenderEffect(() => {
        console.debug("focus");
        setTimeout(() => inputElement.focus(), 1);
    });
}