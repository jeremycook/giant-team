import { h } from '../helpers/h';
import { http } from '../helpers/http';
import { Pipe, State } from '../helpers/Pipe';
import ErrorPage from './_errors/ErrorPage';
import NotFoundPage from "./_errors/NotFoundPage";

export interface IRoutes {
    [k: string]: (..._: any) => Node | Promise<Node>
}

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
    }

    get pipe() {
        return this._state as Pipe<RouteState>;
    }

    /** Returns state as T. */
    get state() {
        return this._state.value.state;
    }

    /** Redirects to a local URL using history.pushState, and pipes the change through this.pipe. */
    redirect(href: string, state?: object) {
        console.debug('State-redirect from {currentHref} to {targetHref}.', location.href, href);

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

export default function Router(routes: IRoutes) {

    // const promiseRejectionReason = new State<any>(undefined);

    // window.addEventListener('unhandledrejection', function (event) {
    //     promiseRejectionReason.value = event.reason;
    // });

    // Sort paths most specific to least specific
    const paths = Object
        .keys(routes)
        .sort((l, r) => r.localeCompare(l));

    const routeRegexes = paths
        .map(p => ({ page: routes[p], regex: new RegExp('^' + p + '$', 'g') }));

    function findRoute(pathname: string) {
        for (const { page, regex } of routeRegexes) {
            regex.lastIndex = 0;
            const result = regex.exec(pathname);
            if (result)
                return { page, routeValues: result.groups ?? {} }
        }
        return undefined;
    }

    async function renderPage(state: RouteState) {
        const pathname = state.pathname;

        const match = findRoute(pathname);

        if (match) {
            const page = match.page;
            try {
                if (page instanceof Node) {
                    return page({ routeValues: match.routeValues });
                }
                else {
                    return await page({ routeValues: match.routeValues });
                }
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

    const child = route.pipe.map(renderPage);
    return h('.site-router', child);
}

/** Handle local links without refreshing the entire page. */
document.addEventListener('click', e => {
    const target = e.target instanceof Element
        ? e.target.closest('a')
        : undefined;

    if (target && http.isLocal(target.href)) {
        e.preventDefault();
        route.redirect(target.href);
    }
})
