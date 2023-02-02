import { h } from '../helpers/h';
import { http } from '../helpers/http';
import { Pipe, State } from '../helpers/Pipe';
import ErrorPage from './_errors/ErrorPage';
import NotFoundPage from "./_errors/NotFoundPage";

interface RouteState {
    pathname: string,
    state: { [k: string]: any },
}

class Route {
    private _state = new State(Object.freeze({
        pathname: location.pathname,
        state: history.state ?? {},
    }));

    constructor() {
        // Update pipe state after pushing state to history.
        window.addEventListener('popstate', e =>
            this._state.value = Object.freeze({
                pathname: location.pathname,
                state: e.state ?? {},
            }));

        // Handle local links without refreshing the entire page.
        document.addEventListener('click', e => {
            const target = e.target instanceof Element
                ? e.target.closest('a')
                : undefined;

            if (target && http.isLocal(target.href)) {
                e.preventDefault();
                this.redirect(target.href);
            }
        })
    }

    get pipe() {
        return this._state as Pipe<RouteState>;
    }

    /** Returns state. */
    get state() {
        return this._state.value.state;
    }

    /** Redirects to a local URL using history.pushState, and pipes the change through this.pipe. */
    redirect(href: string, state?: object) {
        console.debug('Redirecting', location.href, href);

        history.pushState(state ?? {}, '', href);

        // Update pipe state after pushing state to history.
        this._state.value = Object.freeze({
            pathname: location.pathname,
            state: history.state ?? {},
        });
    }
}

/** The global route object. */
export const route = new Route();

export interface IRouteDictionary {
    [k: string]: (..._: any) => Node | Promise<Node>
}

export class RouteDictionary implements IRouteDictionary {
    [k: string]: (..._: any) => Node | Promise<Node>;

}

export class Router {
    private _routeRegexes: { page: (..._: any) => Node | Promise<Node>; regex: RegExp; }[];
    private _pipe: Pipe<Promise<Node>>;

    constructor(route: Route, routes: IRouteDictionary) {
        this._routeRegexes = Object
            .keys(routes)
            .sort((l, r) => r.localeCompare(l))
            .map(p => ({ page: routes[p], regex: new RegExp('^' + p + '$', 'g') }));

        this._pipe = route.pipe.map(x => this._renderPage(x))
    }

    get pipe(): Pipe<Promise<Node>> {
        return this._pipe;
    }

    private _findRoute(pathname: string) {
        for (const { page, regex } of this._routeRegexes) {
            regex.lastIndex = 0;
            const result = regex.exec(pathname);
            if (result)
                return { page, routeValues: result.groups ?? {} }
        }
        return undefined;
    }

    private async _renderPage(state: RouteState) {
        const pathname = state.pathname;

        const match = this._findRoute(pathname);

        if (match) {
            const page = match.page;
            try {
                return await page({ routeValues: match.routeValues });
            } catch (error) {
                debugger;
                return ErrorPage({ error: error });
            }
        }

        // TODO: const canonicalPathname = pathname.toLowerCase();
        // if (paths.includes(canonicalPathname)) {
        //     route.redirect(canonicalPathname);
        //     return FoundPage({ href: canonicalPathname + location.search + location.hash });
        // }

        return NotFoundPage({ href: location.pathname + location.search + location.hash });
    }
}
