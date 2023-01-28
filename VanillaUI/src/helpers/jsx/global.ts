import { elementFactory as jsxElementFactory, fragmentFactory as jsxFragmentFactory } from "../h";
import { JSXElement, IntrinsicElements as jsxIntrinsicElements } from "./jsx";

declare global {
    interface ParentNode {
        append(...nodes: JSX.Element[]): void;
    }

    interface ChildNode {
        replaceWith(...nodes: JSX.Element[]): void;
    }

    var _h: typeof jsxElementFactory
    var _f: typeof jsxFragmentFactory
    namespace JSX {
        type Element = JSXElement;
        interface IntrinsicElements extends jsxIntrinsicElements { }
        interface IntrinsicElements {
            'iconify-icon': any,
        }
    }
}

const _global = (window /* browser */ || globalThis /* node */) as any;
_global._h = jsxElementFactory;
_global._f = jsxFragmentFactory;
