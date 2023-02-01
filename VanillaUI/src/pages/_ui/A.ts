import { BaseNode, h, HAttributes } from '../../helpers/h';

export default function A(
    props: HAttributes,
    ...children: BaseNode[]
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

    return h('a', a => a.set(props), ...children);
}