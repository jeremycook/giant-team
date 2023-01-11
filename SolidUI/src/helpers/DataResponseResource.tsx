import { Resource, ResourceReturn } from "solid-js";
import { DataResponse } from "./httpHelpers";

export class DataResponseResource<T, R = unknown> {
    resource: Resource<DataResponse<T>>;
    refetch: (info?: R | undefined) => DataResponse<T> | Promise<DataResponse<T> | undefined> | null | undefined;

    constructor(resourceReturn: ResourceReturn<DataResponse<T>, R>) {
        const [resouce, { refetch }] = resourceReturn;
        this.resource = resouce;
        this.refetch = refetch;
    }

    public get data(): T | undefined {
        const response = this.resource();
        return response?.ok ? response.data : undefined;
    }
}
