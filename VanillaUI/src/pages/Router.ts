import { http } from '../helpers/http';
import { Pipe, State } from '../helpers/Pipe';
import ErrorPage from './_errors/ErrorPage';
import NotFoundPage from "./_errors/NotFoundPage";

interface RouteState {
    pathname: string,
    state: { [k: string]: any },
}

class Route {
    private static readonly _noState = Object.freeze({});
    private _state = new State({
        pathname: location.pathname,
        state: history.state ?? Route._noState,
    });

    constructor() {
        // Update pipe state after pushing state to history.
        window.addEventListener('popstate', _ => this._setState());

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
        this._setState();
    }

    private _setState() {
        this._state.value = {
            pathname: location.pathname,
            state: history.state ?? Route._noState,
        };
    }
}

/** The global route object. */
export const route = new Route();

export interface IRouteDictionary {
    [k: string]: (..._: any) => Node | Promise<Node>
}

export class Router {
    private _routeRegexes: { page: (..._: any) => Node | Promise<Node>; regex: RegExp; }[];
    private _pipe: Pipe<Promise<Node>>;

    constructor(route: Route, routes: IRouteDictionary) {
        this._routeRegexes = Object
            .keys(routes)
            .sort((l, r) => r.localeCompare(l))
            .map(p => ({ page: routes[p], regex: new RegExp('^' + p + '$', 'g') }));

        this._pipe = route.pipe.project(routeState => this._renderPage(routeState))
    }

    get pagePipe(): Pipe<Promise<Node>> {
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
                console.log(pathname);
                return await page({ routeValues: match.routeValues });
            } catch (error) {
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
