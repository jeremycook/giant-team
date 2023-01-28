import Exception from "../helpers/Exception";
import NotFoundPage from "./_errors/NotFoundPage";
import On from "../helpers/jsx/On";

export interface IRoutes {
    [k: string]: (..._: any) => JSX.Element | Promise<JSX.Element>
}

enum RouteEvent {
    redirect = 'route_redirect',
}

class Route {
    /**
         * Listen for local redirect events. 
         */
    addEventListener(name: RouteEvent, handler: EventListenerOrEventListenerObject) {
        document.addEventListener(name, handler);
    }

    /**
     * Redirects to a local URL using history.pushState,
     * and dispatches the router redirect event to the document.
     */
    redirect(href: string, state?: object) {
        console.debug('Redirecting from ${currentHref} to {targetHref}.', location.href, href);
        history.pushState(state ?? {}, '', href);
        document.dispatchEvent(new CustomEvent(RouteEvent.redirect));
    }

    isLocal(href: string) {
        if (href) {
            if (href.startsWith('/')) {
                return true; // OK
            }

            const compare = location.protocol + '//' + location.host + '/';
            if (href.startsWith(compare)) {
                return true; // Also OK
            }
        }

        return false;
    }

    /** Throws if href is not local. */
    assertLocal(href: string) {
        if (this.isLocal(href)) {
            return; // OK
        }
        throw new Exception(this, 'The {href} argument must be local.', href);
    }
}

export const route = new Route();

export default function Router({ routes }: { routes: IRoutes }) {
    // Sort paths most specific to least specific
    const paths = Object.keys(routes);
    paths.sort((l, r) => r.localeCompare(l))
    const patterns = paths.map(p => new RegExp('^' + p + '$', 'g'));

    return <On
        events={[RouteEvent.redirect, { type: 'popstate', element: window }]}
        class='site-router'
    >{() => {
        const pathname = location.pathname;

        const match = patterns
            .map((p, i) => {
                const result = p.exec(pathname);
                return result ? { i, result } : undefined
            })
            .find(x => x);

        if (match) {
            const path = paths[match.i];
            const route = routes[path];
            return () => route({ routeValues: match.result.groups ?? {}, state: history.state ?? {} });
        }

        // const canonicalPathname = pathname.toLowerCase();
        // if (paths.includes(canonicalPathname)) {
        //     route.redirect(canonicalPathname);
        //     return FoundPage({ href: canonicalPathname + location.search + location.hash });
        // }

        return NotFoundPage({ href: location.pathname + location.search + location.hash });
    }}</On>;
}

/** Handle local links without refreshing the entire page. */
document.addEventListener('click', e => {
    const target = e.target instanceof Element
        ? e.target.closest('a')
        : undefined;

    if (target && route.isLocal(target.href)) {
        e.preventDefault();
        route.redirect(target.href);
    }
})
