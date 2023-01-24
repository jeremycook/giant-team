import { Accessor, createContext, createSignal, ParentProps, Setter, useContext } from "solid-js";
import { apps } from "../../../apps";
import { AppInfo } from "../../../apps/AppInfo";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";

let globalPid = 1;

export class Process {
    private _pid: number;
    private _appInfo: AppInfo;
    private _inode: Accessor<Inode>;
    private _setInode: Setter<Inode>;

    constructor(
        props: {
            pid: number;
            appInfo: AppInfo;
            inode: Inode;
        }
    ) {
        this._pid = props.pid;
        this._appInfo = props.appInfo;
        [this._inode, this._setInode] = createSignal(props.inode);
    }

    get pid() {
        return this._pid;
    }

    get appInfo() {
        return this._appInfo;
    }

    get inode() {
        return this._inode();
    }

    setInode(inode: Inode) {
        this._setInode(inode);
    };
}

export class ProcessOperator {
    private _processes: Accessor<ReadonlyArray<Process>>;
    private _setProcesses: Setter<ReadonlyArray<Process>>;
    private _activePid: Accessor<number | undefined>;
    private _setActivePid: Setter<number | undefined>;

    constructor() {
        [this._processes, this._setProcesses] = createSignal<ReadonlyArray<Process>>([]);
        [this._activePid, this._setActivePid] = createSignal<number>();
    }

    get processes(): ReadonlyArray<Process> {
        return this._processes();
    }

    /** Launch and activate a new process. */
    launch(appInfo: AppInfo, inode: Inode) {
        const process = new Process({
            pid: globalPid++,
            appInfo,
            inode,
        });

        // Add the new process
        this._setProcesses(x => [...x, process]);
        // before activating it
        this.activate(process.pid);

        return process.pid;
    }

    /** Activates the first process that matches inode, or launches a new process if no match was found. */
    open(inode: Inode) {
        const runningProcess = this.processes.find(x => x.inode.inodeId === inode.inodeId);
        if (runningProcess) {
            // If a matching process is already running just activate it
            this.activate(runningProcess.pid);
            return runningProcess;
        }
        else {
            // Launch a new process
            const appInfo = apps.find(x => x.canHandle(inode));
            if (!appInfo) throw Error('No app was found that can handle: ' + inode.inodeTypeId);
            return this.launch(appInfo, inode);
        }
    }

    get activePid() {
        const pid = this._activePid();
        return pid;
    }

    activate(pid: number) {
        // Only activate if the pid matches a running process
        if (this.processes.some(x => x.pid === pid)) {
            this._setActivePid(pid)
        }
    }

    terminate(pid: number) {
        // Deactivate the process
        if (this.activePid == pid) {
            const subset = this.processes.filter(x => x.pid !== pid);
            const newPid = subset.length > 0 ?
                subset[subset.length - 1].pid :
                undefined;
            this._setActivePid(newPid);
        }

        // Before removing it
        this._setProcesses(x => x.filter(y => y.pid !== pid));
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