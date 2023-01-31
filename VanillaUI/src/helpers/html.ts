import Exception from './Exception';

export type Elem =
    | boolean
    | number
    | string
    | Node
    | ((parent: Elem) => Elem);

export type ElemAttributeValue =
    | boolean
    | number
    | string
    | EventListenerOrEventListenerObject;

export type ElemAttributes =
    | { [k: string]: ElemAttributeValue };

export type HtmlAsyncProps = (ElemAttributes & { children?: Elem | Promise<Elem> | (Elem | Promise<Elem>)[] });

export async function htmlAsync<HTMLTagName extends keyof HTMLElementTagNameMap>(
    tagName: HTMLTagName,
    props?: HtmlAsyncProps
): Promise<HTMLElementTagNameMap[HTMLTagName]> {

    const element = document.createElement(tagName);

    if (props) {
        const { children, ...attributes } = props;

        setAttributes(element, attributes);

        if (children) {
            if (Array.isArray(children)) {
                for (const child of children) {
                    await tryAppendChildAsync(element, child);
                }
            }
            else {
                await tryAppendChildAsync(element, children);
            }
        }
    }

    return element;
}

export class ElementEditor {
    constructor(
        public element: Element) {
    }

    append(...children: (Elem | Elem[])[]) {
        for (const child of children.flat()) {
            tryAppendChild(this.element, child);
        }
        return this;
    }

    async appendAsync(...children: (Elem | Promise<Elem> | (Elem | Promise<Elem>)[])[]) {
        for (const child of children.flat()) {
            await tryAppendChildAsync(this.element, child);
        }
        return this;
    }

    set(attributes?: ElemAttributes) {
        if (attributes) {
            setAttributes(this.element, attributes);
        }
        return this;
    }
}

export function html<HTMLTagName extends keyof HTMLElementTagNameMap>(
    tagName: HTMLTagName,
    editor?: (html: ElementEditor) => void,
): HTMLElementTagNameMap[HTMLTagName] {

    const element = document.createElement(tagName);

    if (editor) {
        const customizer = new ElementEditor(element);
        editor(customizer);
    }

    return element;
}

/** Returns `true` if the child was appended. */
export function tryAppendChild(parent: ParentNode, child: Elem): boolean {
    let appended = false;

    if (child instanceof Node) {
        parent.appendChild(child);
        appended = true;
    }
    else {
        switch (typeof child) {
            case 'string':
                parent.appendChild(document.createTextNode(child));
                appended = true;
                break;
            case 'number':
                parent.appendChild(document.createTextNode(child.toString()));
                appended = true;
                break;
            case 'function':
                const result = child(parent);
                if (tryAppendChild(parent, result)) {
                    appended = true;
                }
                break;
        }
    }

    return appended;
}

/** Returns `true` if the child was appended. */
export async function tryAppendChildAsync(parent: ParentNode, child: Elem | Promise<Elem>): Promise<boolean> {
    let appended = false;

    if (child instanceof Node) {
        parent.appendChild(child);
        appended = true;
    }
    else if (child instanceof Promise) {
        const result = await child;
        if (await tryAppendChildAsync(parent, result)) {
            appended = true;
        }
    }
    else {
        switch (typeof child) {
            case 'string':
                parent.appendChild(document.createTextNode(child));
                appended = true;
                break;
            case 'number':
                parent.appendChild(document.createTextNode(child.toString()));
                appended = true;
                break;
            case 'function':
                const result = child(parent);
                if (await tryAppendChildAsync(parent, result)) {
                    appended = true;
                }
                break;
        }
    }

    return appended;
}

function setAttributes(element: Element, attributes: ElemAttributes) {
    const tagName = element.tagName;

    for (const [name, value] of Object.entries(attributes)) {

        switch (typeof value) {
            case 'string':
                element.setAttribute(name, value);
                break;

            case 'number':
                element.setAttribute(name, value.toString());
                break;

            case 'boolean':
                if (value)
                    element.setAttribute(name, '');
                break;

            case 'function':
                if (name.startsWith('on')) {
                    element.addEventListener(
                        name.substring(2, 3).toLowerCase() + name.substring(3),
                        value);
                }
                else {
                    throw new Exception(setAttributes, 'Unsupported attribute type. Function attributes must start with "on".', { tagName, name, value, typeof: typeof value });
                }
                break;

            default:
                throw new Exception(setAttributes, 'Unsupported attribute type.', { tagName, name, value, typeof: typeof value });
        }
    }
}
