import { Inode } from '../../../bindings/Organization.Etc.Models';
import { createStore } from '../../../helpers/StoreProxyHandler';
import { apps } from '../apps';
import { AppInfo } from '../apps/AppInfo';

let globalPid = 1;

export class Process {
    private _pid: number;
    private _appInfo: AppInfo;
    private _state: {
        inode: Inode
    };

    constructor(
        props: {
            pid: number;
            appInfo: AppInfo;
            inode: Inode;
        }
    ) {
        this._pid = props.pid;
        this._appInfo = props.appInfo;
        this._state = createStore({ inode: props.inode });
    }

    get pid() {
        return this._pid;
    }

    get appInfo() {
        return this._appInfo;
    }

    get inode() {
        return this._state.inode;
    }

    setInode(inode: Inode) {
        this._state.inode = inode;
    };
}

export class ProcessOperator {
    private _state = createStore({
        processes: [] as ReadonlyArray<Process>,
        activePid: null as number | null,
    });

    get processes(): ReadonlyArray<Process> {
        return this._state.processes;
    }

    /** Launch and activate a new process. */
    launch(appInfo: AppInfo, inode: Inode) {
        const process = new Process({
            pid: globalPid++,
            appInfo,
            inode,
        });

        // Add the new process
        this._state.processes = [...this._state.processes, process];
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
        const pid = this._state.activePid;
        return pid;
    }

    activate(pid: number) {
        // Only activate if the pid matches a running process
        if (this.processes.some(x => x.pid === pid)) {
            this._state.activePid = pid;
        }
    }

    terminate(pid: number) {
        // Deactivate the process
        if (this.activePid == pid) {
            const subset = this.processes.filter(x => x.pid !== pid);
            const newPid = subset.length > 0 ?
                subset[subset.length - 1].pid :
                null;
            this._state.activePid = newPid;
        }

        // And then remove it
        this._state.processes = this._state.processes.filter(y => y.pid !== pid);
    }
}
