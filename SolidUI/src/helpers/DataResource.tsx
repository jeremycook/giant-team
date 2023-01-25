import { Resource, ResourceReturn } from "solid-js";

export class DataResource<T, R = unknown> {
    resource: Resource<T>;
    refetch: (info?: R | undefined) => T | Promise<T | undefined> | null | undefined;
    mutate: ({} | (() => undefined)) & (<U extends T | undefined>(value: (prev: T | undefined) => U) => U) & (<U extends T | undefined>(value: Exclude<U, Function>) => U) & (<U extends T | undefined>(value: Exclude<U, Function> | ((prev: T | undefined) => U)) => U);

    constructor(resourceReturn: ResourceReturn<T, R>) {
        const [resouce, { refetch, mutate }] = resourceReturn;
        this.resource = resouce;
        this.refetch = refetch;
        this.mutate = mutate;
    }

    public get data() {
        const response = this.resource();
        return response;
    }
}
