import { createContext, JSX, onCleanup, onMount, ParentProps, useContext } from "solid-js";
import { createStore, SetStoreFunction } from "solid-js/store";

export function Section(props: { name: string } & ParentProps) {
    const sectionContext = useSectionContext();

    onMount(() => sectionContext.set(props.name, props.children));
    onCleanup(() => sectionContext.unset(props.name));

    return (<></>)
}

export function RenderSection(props: { name: string }) {
    const sectionContext = useSectionContext();
    return (<>
        {sectionContext.get(props.name)}
    </>)
}

export class SectionContextValue {
    _context: Record<string, JSX.Element>;
    _setContext: SetStoreFunction<Record<string, JSX.Element>>;

    constructor() {
        [this._context, this._setContext] = createStore({});
    }

    get(name: string) {
        const sectionStack = this._context[name];
        return sectionStack;
    }

    set(name: string, element: JSX.Element) {
        this._setContext(name, e => element)
    }

    unset(name: string) {
        this.set(name, undefined);
    }
}

export const SectionContext = createContext(new SectionContextValue());

export function useSectionContext() { return useContext(SectionContext); }
