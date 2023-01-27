import { AnchorHTMLAttributes, JSXElement } from "../../helpers/jsx/jsx";

export default function (
    props: AnchorHTMLAttributes<HTMLAnchorElement>,
    ...children: JSXElement[]
) {
    const currentPathname = location.pathname;
    const href = (props.href as string | undefined);

    const currentArea = href?.startsWith(currentPathname);
    if (currentArea) {
        props.class = (props.class ?? '') + ' current-area';

        if (currentPathname == href) {
            props["aria-current"] = 'page';
            props.class = (props.class ?? '') + ' current-page';
        }
    }

    return <a {...props}>{children}</a>;
}