import { Accessor, createContext, createSignal, ParentProps, Setter, useContext } from "solid-js";
import { AppInfo } from "../../../apps/AppInfo";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { immute } from "../../../helpers/immute";

export interface Process {
    appInfo: AppInfo;
    inode: Inode;
}

export class ProcessOperator {
    private _processes: Accessor<Process[]>;
    private _setProcesses: Setter<Process[]>;
    private _activeIndex: Accessor<number>;
    private _setActiveIndex: Setter<number>;

    constructor() {
        [this._processes, this._setProcesses] = createSignal<Process[]>([]);
        [this._activeIndex, this._setActiveIndex] = createSignal(-1);
    }

    get processes(): ReadonlyArray<Process> {
        return this._processes();
    }

    launch(appInfo: AppInfo, inode: Inode) {
        const newProcess = { appInfo, inode };
        this._setProcesses(x => [...x, newProcess]);
        this.activateByIndex(this.processes.length - 1);
        return this.processes.length;
    }

    get activeIndex(): number {
        // Ensure the returned index is always in range
        return Math.min(this.processes.length - 1, this._activeIndex());
    }

    activateByIndex(index: number) {
        this._setActiveIndex(Math.min(this.processes.length - 1, index))
    }

    terminateByIndex(index: number) {
        this._setProcesses(x => {
            return immute.removeAt(x, index);
        });
    }
}

export const ProcessOperatorContext = createContext(new ProcessOperator());

export function useProcessOperatorContext() { return useContext(ProcessOperatorContext); }

export function ProcessOperatorProvider(props: { processOperator: ProcessOperator } & ParentProps) {
    return (
        <ProcessOperatorContext.Provider value={props.processOperator}>
            {props.children}
        </ProcessOperatorContext.Provider>
    );
}