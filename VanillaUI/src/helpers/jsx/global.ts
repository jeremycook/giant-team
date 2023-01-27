import { elementFactory as jsxElementFactory, fragmentFactory as jsxFragmentFactory } from "../h";
import { JSXElement, IntrinsicElements as jsxIntrinsicElements } from "./jsx";

declare global {
    interface ParentNode {
        append(...nodes: JSXElement[]): void;
    }

    namespace JSX {
        var factory: typeof jsxElementFactory
        var fragmentFactory: typeof jsxFragmentFactory
        interface IntrinsicElements extends jsxIntrinsicElements { }
        interface IntrinsicElements {
            'iconify-icon': any,
        }
    }
}

const _global = (window /* browser */ || globalThis /* node */) as any;
_global.JSX ??= {};
_global.JSX.factory = jsxElementFactory;
_global.JSX.fragmentFactory = jsxFragmentFactory;
