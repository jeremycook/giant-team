import { Accessor, createSignal, Setter } from "solid-js";
import { Inode } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { FetchOrganizationDetailsApp } from "../../bindings/GiantTeam.Organization.Services";
import { immute } from "../../helpers/immute";


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
        });
    }
}
export interface App extends FetchOrganizationDetailsApp { }
export interface Process {
    app: App;
    inode: Inode;
}
