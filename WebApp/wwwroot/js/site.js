function autofocus() {
    const source = document.querySelector("[data-autofocus]");
    if (source) {
        const autofocusSelector = source.getAttribute("data-autofocus");
        const target = source.querySelector(autofocusSelector);
        target.focus();
    }
}
autofocus();
