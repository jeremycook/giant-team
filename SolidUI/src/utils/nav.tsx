import { A } from "@solidjs/router";
import { createSignal, createContext, useContext, JSX, createEffect, Accessor, Show, For, untrack, onMount, onCleanup } from "solid-js";
import { unwrap } from "solid-js/store";
import { removeItem } from "../helpers/arrayHelpers";

export interface Link {
    text: string,
    href: string,
}

function makeBreadcrumbContext() {

    const [links, setLinks] = createSignal<Link[]>([]);

    function push(link: Link) {
        setLinks(c => [...c, link]);
    };

    function pop() {
        const copy = [...untrack(links)];
        const popped = copy.pop();

        setLinks(copy);

        return popped;
    }

    function remove(link: Link) {
        setLinks(c => {
            const copy = [...c];
            removeItem(copy, link);
            return copy;
        });
    }

    return { links, push, pop, remove } as const;
}

type BreadcrumbContextType = ReturnType<typeof makeBreadcrumbContext>;

const BreadcrumbContext = createContext<BreadcrumbContextType>();

export const useBreadcrumbContext = () => useContext(BreadcrumbContext)!;

export interface BreadcrumbContextProps {
    modifyPageTitle?: boolean,
    children: JSX.Element;
}

export function BreadcrumbProvider(props: BreadcrumbContextProps) {
    const breadcrumb = makeBreadcrumbContext();

    if (props.modifyPageTitle) {
        createEffect(() => {
            document.title = breadcrumb.links().map(l => l.text).join(' • ') ?? 'Welcome';
        })
    }

    return (
        <BreadcrumbContext.Provider value={breadcrumb}>
            {props.children}
        </BreadcrumbContext.Provider>
    );
}

export interface BreadcrumbProps {
    link: Link,
}

export function Breadcrumb(props: BreadcrumbProps) {
    const breadcrumbs = useBreadcrumbContext();

    onMount(() => {
        breadcrumbs.push(props.link);
        console.debug("Breadcrumb pushed", props.link)
    });

    onCleanup(() => {
        const popped = breadcrumbs.pop();
        console.debug("Breadcrumb popped", popped)
    })

    return null;
}

export function BreadcrumbTrail() {
    const breadcrumbs = useBreadcrumbContext();

    return (<>
        <For each={breadcrumbs.links()}>{breadcrumb => <A href={breadcrumb.href}>{breadcrumb.text}</A>}</For>
    </>)
}
