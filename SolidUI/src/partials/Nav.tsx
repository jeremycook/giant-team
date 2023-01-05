import { ParentProps, ValidComponent } from "solid-js";
import { createStore } from "solid-js/store";
import { JSX } from "solid-js/web/types/jsx";
import { log } from "../helpers/logging";

const pagesRoot = '/src/pages';
const pagesExt = '.tsx';
const pages: Record<string, any> = (import.meta as any).glob(['/src/pages/**/*.tsx', '!/**/partials/*', '!/**/_*', '!/**/.*'], { eager: true });

export interface PageSettings {
    name: string | (() => string);
    showInNav?: () => boolean;
}

export interface PageRoute {
    key: string;
    route: string;
    routeKeys: string[];
    pattern: RegExp;
    name: string | (() => string);
    component: ValidComponent;
}

interface HereState {
    [x: string]: string | undefined;
}

interface HereRouteValues {
    [x: string]: string | undefined;
}

export interface Here {
    pathname: string;
    routeValues: HereRouteValues;
    state: HereState;
}

const _routeKeyPattern = /\{([^\}]*)\}/g;

/** Store getter */
export const [pageRoutes,] = createStore<PageRoute[]>(Object.entries(pages)
    .map(([key, p]) => {
        const pageSettings = p.pageSettings as PageSettings | undefined;

        const route = key
            .substring(pagesRoot.length, key.length - pagesExt.length)
            .replace(/\/index$/i, '')
            .replace(/^$/, '/');

        const routeKeys: string[] = [];

        const pattern = new RegExp('^' + route.replaceAll(_routeKeyPattern, (_, name) => {
            routeKeys.push(name);
            return `(?<${name}>[^/]+)`;
        }) + '$');

        return {
            key,
            route,
            routeKeys,
            pattern,
            name: pageSettings?.name ?? route,
            component: p.default as ValidComponent
        }
    })
    .sort((l, r) => l.route.replaceAll('{', '\n').localeCompare(r.route.replaceAll('{', '\n'))));

export const routeLookup = pageRoutes.reduce((obj, item) => ({ ...obj, [item.route]: item }), {});

const [_here, _setHere] = createStore<Here>(_calcHere(location.pathname));

/** Store getter */
export const here = _here;

function _calcHere(pathname: string) {
    console.debug('_calcHere', pathname);

    const page = findPage(pathname);

    const routeValues = page ?
        { ...page.pattern.exec(pathname)?.groups } :
        {};

    return {
        pathname: pathname,
        routeValues,
        state: history.state ?? {},
    };
}

function _refreshHere() {
    _setHere(_calcHere(location.pathname));
}

/** Returns a PageRoute that matches pathname. */
export function findPage(pathname: string) {
    for (let i = pageRoutes.length - 1; i >= 0; i--) {
        const pageRoute = pageRoutes[i];
        if (pageRoute.pattern.test(pathname)) {
            return pageRoute;
        }
    }
    return undefined;
}

/** Blends route and routeValues to create a URL. */
export function pageUrl(route: string, routeValues: HereRouteValues) {

    const missingRouteValues: string[] = [];
    const url = route.replaceAll(_routeKeyPattern, (_, name) => {
        if (name in routeValues)
            return routeValues[name] ?? '';
        else {
            missingRouteValues.push(name);
            return '';
        }
    });

    if (missingRouteValues.length > 0) {
        log.error('Failed to generate a URL for {route}. These route values were missing: {missingRouteValues}', { route, missingRouteValues });
        return '';
    }
    else {
        return url;
    }
}

/** Navigate with history.pushState(...) */
export function go(url: string | URL, state?: { [x: string]: string | undefined }) {
    history.pushState(state, '', url);
    _refreshHere();
}

export function A(props: { href: string, stateData?: any, onclick?: never } & JSX.AnchorHTMLAttributes<HTMLAnchorElement> & ParentProps) {
    const { children, href, stateData, ...attributes } = props;

    const aOnClick = (e: MouseEvent & { currentTarget: HTMLAnchorElement; target: Element; }): void => {
        e.preventDefault();
        go(props.href, props.stateData);
    };

    return (
        <a href={props.href} onclick={aOnClick} {...attributes}>
            {props.children}
        </a>
    )
}

// SIDE-EFFECTS

addEventListener('popstate', () => {
    _refreshHere();
});
