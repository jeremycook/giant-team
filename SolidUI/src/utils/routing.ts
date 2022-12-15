import { batch } from "solid-js";
import { createStore } from "solid-js/store";

var dispatchHistoryEvent = function (type: 'pushState' | 'replaceState') {
    var original = history[type];
    return () => {
        var wrapper = original.apply({}, arguments as any);
        var e = new Event(type);
        (e as any).arguments = arguments;
        window.dispatchEvent(e);
        return wrapper;
    };
};
history.pushState = dispatchHistoryEvent('pushState');
history.replaceState = dispatchHistoryEvent('replaceState');

document.addEventListener('replaceState', e => {
    refreshRouteValues();
})

document.addEventListener('pushState', e => {
    refreshRouteValues();
})

function dictWithCanonicalKeys<V>(canon: (prop: keyof any) => keyof any) {
    return new Proxy<{ [k: string]: V }>(
        {},
        {
            get(target, prop) {
                return Reflect.get(target, canon(prop));
            },
            set(target, prop, value) {
                return Reflect.set(target, canon(prop), value);
            },
            has(target, prop) {
                return Reflect.has(target, canon(prop));
            },
            defineProperty(target, prop, attribs) {
                return Reflect.defineProperty(target, canon(prop), attribs);
            },
            deleteProperty(target, prop) {
                return Reflect.deleteProperty(target, canon(prop));
            },
            getOwnPropertyDescriptor(target, prop) {
                return Reflect.getOwnPropertyDescriptor(target, canon(prop));
            }
        }
    );
}

export class RouteDictionary implements Record<string, string> {
    constructor() {
        return dictWithCanonicalKeys<string>(
            p => (typeof p === "string" ? p.toLowerCase() : p)
        );
    }

    [x: string]: string;
};

const [getRouteValues, set] = createStore({
    pathname: location.pathname,
    params: {} as Record<string, string>,
    state: history.state,
});

export function refreshRouteValues() {
    const updatedParams = new RouteDictionary();
    var searchParams = new URLSearchParams(location.search);
    searchParams.forEach((value, key) => {
        updatedParams[key] = value || '';
    });

    batch(() => {
        set('pathname', location.pathname);
        set('params', updatedParams);
        set('state', history.state);
    })
}

export const routeValues = getRouteValues;

// TODO: export const routeTo = (pathname: string, { data, params }: { data: any, params: Record<string, any> }) => {
//     history.pushState(data, '', createUrl(pathname, params));
// }
