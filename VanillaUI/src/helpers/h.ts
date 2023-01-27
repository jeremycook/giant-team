import { SVGElementTags, HTMLElementTags, JSXElement, FunctionElement } from "./jsx/jsx";

export function appendChild(parent: ParentNode, child: JSXElement | JSXElement[]) {
    if (typeof child === 'undefined' || child === null) {
        // Render nothing
    }
    else if (child instanceof Node) {
        parent.appendChild(child);
    }
    else if (child instanceof Array) {
        for (const value of child) {
            appendChild(parent, value);
        }
    }
    else if (child instanceof Promise) {
        child.then(result => appendChild(parent, result));
    }
    else if (child instanceof Function) {
        appendChild(parent, (child as FunctionElement)());
    }
    else {
        switch (typeof child) {
            case 'string':
                parent.appendChild(document.createTextNode(child));
                break;
            case 'number':
                parent.appendChild(document.createTextNode(child.toString()));
                break;
            case 'function':
                appendChild(parent, (child as Function)(parent));
                break;
            default:
                throw { message: 'Function attributes that do not start with "on" are not supported.', typeof: typeof child, parent, child };
        }
    }
}

const attributeNamePatches: { [k: string]: { newKey: string, tagNames?: Set<string> } } = {
    className: { newKey: 'class' },
    vectorEffect: { newKey: 'vector-effect', tagNames: new Set<string>(['circle', 'ellipse', 'foreignObject', 'image', 'line', 'path', 'polygon', 'polyline', 'rect', 'text', 'textPath', 'tspan', 'use']) },
};

const getAttributeName = (tagName: string, attributeName: string) => {
    const patch = attributeNamePatches[attributeName];
    if (patch && (!patch.tagNames || patch.tagNames.has(tagName))) {
        return patch.newKey;
    }
    return attributeName;
}

function setAttributes(element: Element, attributes: { [k: string]: any }) {
    const tagName = element.tagName;

    for (const [key, value] of Object.entries(attributes)) {
        const name = getAttributeName(tagName, key);

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
                        value as EventListenerOrEventListenerObject);
                }
                else {
                    throw { message: 'Function attributes must start "on".', tagName, name, value, typeof: typeof value };
                }
                break;

            default:
                throw { message: 'Unsupported attribute type.', tagName, name, value, typeof: typeof value };
                break;
        }
    }
}

export function createHTMLElement<HTMLTagName extends keyof HTMLElementTags>(
    tagName: HTMLTagName,
    attributes?: HTMLElementTags[HTMLTagName] | null,
    ...children: JSXElement[]
): HTMLElementTagNameMap[HTMLTagName] {
    const element = document.createElement(tagName);

    if (attributes) {
        setAttributes(element, attributes);
    }

    for (const child of children) {
        appendChild(element, child);
    }

    return element;
}

export function createSVGElement<SVGTagName extends keyof SVGElementTags>(
    tagName: SVGTagName,
    attributes?: SVGElementTags[SVGTagName] | null,
    ...children: SVGElement[]
): SVGElementTagNameMap[SVGTagName] {
    const element = document.createElementNS('http://www.w3.org/2000/svg', tagName);

    if (attributes) {
        setAttributes(element, attributes);
    }

    for (const child of children) {
        appendChild(element, child);
    }

    return element;
}

export function elementFactory<HTMLTagName extends keyof HTMLElementTags>(
    tagName: HTMLTagName,
    attributes?: HTMLElementTags[HTMLTagName] | null,
    ...children: JSXElement[]
): HTMLElementTagNameMap[HTMLTagName];
export function elementFactory(
    tagName: string | ((attributes?: { [k: string]: any }, ...children: JSXElement[]) => JSXElement),
    attributes?: (attributes: { [k: string]: any }, ...children: JSXElement[]) => JSXElement | null,
    ...children: JSXElement[]
) {
    if (typeof tagName === 'function') {
        return tagName(attributes, children);
    }
    else {
        return createHTMLElement(tagName as keyof HTMLElementTags, attributes as any, ...children);
    }
}

export function fragmentFactory(
    attributes?: never,
    ...children: JSXElement[]
) {
    // const fragment = document.createDocumentFragment();
    // for (const child of children) {
    //     appendChild(fragment, child);
    // }
    return children;
};

export {
    createHTMLElement as h,
    createSVGElement as svg,
};