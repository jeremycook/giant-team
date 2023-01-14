import { RouteDataFuncArgs, useRouteData } from "@solidjs/router";
import { Accessor, createResource, createSignal, Setter } from "solid-js";
import { postFetchInode, postFetchOrganizationDetails } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { FetchInodeResult, FetchOrganizationDetailsApp, FetchOrganizationDetailsResult } from "../../bindings/GiantTeam.Organization.Services";
import { DataResponseResource } from "../../helpers/DataResponseResource";
import { immute } from "../../helpers/immute";

export interface App extends FetchOrganizationDetailsApp { }

export interface Process {
    app: App;
    inode: Inode;
}

export class ProcessOperator {
    private _processes: Accessor<Process[]>;
    private _setProcesses: Setter<Process[]>;

    constructor() {
        [this._processes, this._setProcesses] = createSignal<Process[]>([]);
    }

    get processes(): ReadonlyArray<Process> {
        return this._processes();
    }

    launch(app: FetchOrganizationDetailsApp, inode: Inode) {
        this._setProcesses(x => [...x, { app, inode }]);
    }

    terminateByIndex(index: number): void {
        this._setProcesses(x => {
            return immute.removeAt(x, index);
        })
    }
}

export class OrganizationDetailsResource extends DataResponseResource<FetchOrganizationDetailsResult>{ }

export class InodeResource extends DataResponseResource<FetchInodeResult>{ }

export class OrganizationOperator {
    private _processes: Accessor<Process[]>;
    private _setProcesses: Setter<Process[]>;
    private _organizationDetailsResource: OrganizationDetailsResource;
    private _inodeResource: InodeResource;

    constructor(
        organizationDetailsResource: OrganizationDetailsResource,
        inodeResource: InodeResource
    ) {
        [this._processes, this._setProcesses] = createSignal<Process[]>([]);
        this._organizationDetailsResource = organizationDetailsResource;
        this._inodeResource = inodeResource;
    }

    get organization() {
        return this._organizationDetailsResource.data!;
    }

    get inode() {
        return this._inodeResource.data?.inode;
    }

    get processes(): ReadonlyArray<Process> {
        return this._processes();
    }

    launchProcess(app: FetchOrganizationDetailsApp, inode: Inode) {
        this._setProcesses(x => [...x, { app, inode }]);
    }

    closeProcessAt(index: number): void {
        this._setProcesses(x => {
            return immute.removeAt(x, index);
        })
    }
}

export function createInodeResource(props: { organization: string, path: string }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization, path: props.path }),
        (props) => postFetchInode(props)
    );
    return new InodeResource(resourceReturn);
}

export function createOrganizationDetailsResource(props: { organization: string }) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization }),
        (props) => postFetchOrganizationDetails(props)
    );
    return new OrganizationDetailsResource(resourceReturn);
}

export function createOrganizationOperatorRouteData({ params }: RouteDataFuncArgs) {
    const [resource] = createResource(
        () => ({ organization: params.organization, path: params.path }),
        (props) => new OrganizationOperator(
            createOrganizationDetailsResource(props),
            createInodeResource(props)
        )
    );
    return resource();
}

export function useOrganizationOperatorRouteData() {
    return useRouteData<OrganizationOperator>();
}
