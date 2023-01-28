export function createStore<T extends object>(target: T) {
    return new Proxy<T>(target, new StoreProxyHandler<T>());
}

export class StoreProxyHandler<T extends object> implements ProxyHandler<T> {
    // TODO
}