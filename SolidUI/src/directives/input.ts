import { createRenderEffect } from "solid-js";

declare module "solid-js" {
    namespace JSX {
        interface Directives {
            input: [() => any, (v: any) => any];
        }
    }
}

export const input = (
    inputElement: HTMLInputElement,
    prop: () => [() => any, (v: any) => any]) => {

    const [value, setValue] = prop();

    createRenderEffect(() => (inputElement.value = value()));

    inputElement.addEventListener("input", function () { setValue(this.value) });
}