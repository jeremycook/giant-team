import NotFoundPage from "./_errors/NotFoundPage";
import HomePage from "./HomePage"
import LoginPage from "./login/LoginPage";
import FoundPage from "./_errors/FoundPage";
import Exception from "../helpers/Exception";
import LogoutPage from "./login/LogoutPage";
import On from "../helpers/jsx/On";

const routes: { [k: string]: (..._: any) => any } = {
    '/': HomePage,
    '/login': LoginPage,
    '/logout': LogoutPage,
}

const paths = Object.keys(routes);

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
    redirect(href: string) {
        console.debug('Redirecting from ${currentHref} to {targetHref}.', location.href, href);
        history.pushState({}, '', href);
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

export default function Router() {
    return <On event={[RouteEvent.redirect]}>{() => {
        const pathname = location.pathname;

        if (paths.includes(pathname)) {
            return routes[pathname]({ state: history.state ?? {} });
        }

        const canonicalPathname = pathname.toLowerCase();
        if (paths.includes(canonicalPathname)) {
            route.redirect(canonicalPathname);
            return FoundPage({ href: canonicalPathname + location.search + location.hash });
        }

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
