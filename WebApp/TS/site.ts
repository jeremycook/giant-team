// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Focuses on the first child of the element this is applied to that matches its data-autofocus attribute.
 * Usage: <div data-autofocus="#SomeElementSelector"><input id="SomeElementSelector" /></div>
 */
function autofocus() {
    const source = document.querySelector("[data-autofocus]");
    if (source) {
        const autofocusSelector = source.getAttribute("data-autofocus");
        const target: HTMLElement = source.querySelector(autofocusSelector);
        target.focus();
    }
}

autofocus();