import { ParentProps } from "solid-js";
import { createStore } from "solid-js/store";
import { JSX } from "solid-js/web/types/jsx";

const [_here, _setHere] = createStore({
    pathname: location.pathname,
    state: history.state,
});

export const here = _here;

export interface PageInfo {
    name: string | (() => string),
    showInNav?: () => boolean,
};

export function getState<TState>(): TState | undefined {
    return _here.state;
}

export function go(url: string | URL, state?: any) {
    window.history.pushState(state, '', url);
    _setHere({
        pathname: location.pathname,
        state: history.state,
    });
}

export function A(props: { href: string, stateData?: any, onclick?: never } & JSX.AnchorHTMLAttributes<HTMLAnchorElement> & ParentProps) {
    const { children, href, stateData, ...attributes } = props;

    const aOnClick = (e: MouseEvent & { currentTarget: HTMLAnchorElement; target: Element; }): void => {
        e.preventDefault();
        go(props.href, props.stateData);
    };

    return (
        <a onclick={aOnClick} {...attributes}>
            {props.children}
        </a>
    )
}