const svgTagNames = new Set(["svg", "line"]);

/**
 * Create an element with attributes and content.
 * @param tag
 * @param data An array of strings, Nodes and attribute objects.
 */
export function e(tag: string, ...data:
    (
        string |
        Node |
        (string | Node)[] |
        { ["style"]: { [key: string]: string } } |
        { [key: string]: string | EventListener | { [key: string]: string } }
    )[]): Node {
    const tagNameMatch = /^[a-z][a-z-]*/i.exec(tag);

    const tagName = tagNameMatch !== null ? tagNameMatch[0] : "div";

    const element = svgTagNames.has(tagName) ?
        document.createElementNS("http://www.w3.org/2000/svg", tagName) :
        document.createElement(tagName);

    const idMatch = /#([a-z][a-z-_]*)/i.exec(tag);
    if (idMatch !== null) {
        element.id = idMatch[1];
    }

    const classPattern = /\.([a-z][a-z-_]*)/gi;
    let classesMatch: RegExpExecArray;
    while ((classesMatch = classPattern.exec(tag)) && classesMatch !== null) {
        element.classList.add(classesMatch[1]);
    }

    for (let i = 0; i < data.length; i++) {
        const content = data[i];

        if (typeof content === "string") {
            element.appendChild(t(content));
        }
        else if (content instanceof Node) {
            element.appendChild(content);
        }
        else if (content instanceof Array) {
            for (const node of content) {
                if (typeof node === "string") {
                    element.appendChild(t(node));
                }
                else {
                    element.appendChild(node);
                }
            }
        }
        else {
            for (const name in content) {
                if (name === "style") {
                    const val = content[name];
                    if (typeof val === "object") {
                        Object.getOwnPropertyNames(val).forEach(prop => element.style.setProperty(prop, val[prop]));
                    }
                    else {
                        throw `The "${name}" attribute of type "${typeof val}" is not supported.`;
                    }
                }
                else {
                    const val = (<any>content)[name];

                    if (typeof val === "string") {
                        element.setAttribute(name, val);
                    }
                    else if (typeof val === "function") {
                        element.addEventListener(name.substr(2), val);
                    }
                    else {
                        throw `The "${name}" attribute of type "${typeof val}" is not supported.`;
                    }
                }
            }
        }
    }

    return element;
}

/**
 * Create a text node.
 * @param content
 */
export function t(content: string) {
    return document.createTextNode(content);
}

/**
 * Create a comment node.
 * @param content
 */
export function c(content: string) {
    return document.createComment(content);
}
