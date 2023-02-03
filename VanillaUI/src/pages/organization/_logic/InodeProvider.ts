import { fetchInodeList } from '../../../bindings/Organization.Api.Controllers';
import { OrganizationDetails, Inode, InodeTypeId } from '../../../bindings/Organization.Etc.Models';
import Exception from '../../../helpers/Exception';
import { log } from '../../../helpers/log';
import { State } from '../../../helpers/Pipe';

export class InodeModel {
    _inode: string;
    _parentInodeId: string;
    _inodeTypeId: string;
    _name: State<string>;
    _uglyName: string;
    _created: Date;
    _path: string;

    constructor(data: Inode) {
        this._inode = data.inodeId;
        this._parentInodeId = data.parentInodeId;
        this._inodeTypeId = data.inodeTypeId;
        this._name = new State(data.name);
        this._uglyName = data.uglyName;
        this._created = data.created ?? new Date();
        this._path = data.path;
    }

    get inodeId(): string {
        return this._inode;
    }
    get parentInodeId(): string {
        return this._parentInodeId;
    }
    get inodeTypeId(): InodeTypeId {
        return this.inodeTypeId;
    }
    get name(): State<string> {
        return this._name;
    }
    get uglyName(): string {
        return this._uglyName;
    }
    get created(): Date {
        return this._created;
    }
    get path(): string {
        return this._path;
    }
}

export class InodeProvider {
    private _organization: OrganizationDetails;
    private _state = new State<InodeModel[]>([]);
    private _inodes = this._state.asArray();

    constructor(
        { organization }: { organization: OrganizationDetails }) {
        this._organization = organization;
        this.refresh();
    }

    /** Returns the root inode (path is ''). */
    get root() {
        return this.inode('');
    }

    /** Returns the inode at {@link path} or throws. */
    inode(path: string): InodeModel {
        const inode = this._inodes.value.find(i => i.path === path);
        if (!inode)
            throw new Exception(this.inode, 'Inode not found at {path}.', path);
        return inode;
    }
    /** Returns the children of the inode at {@link path}. */
    children(path: string): InodeModel[] {
        const parentInodeId = this.inode(path).inodeId;
        const inodeList = this._inodes.value
            .filter(i => i.parentInodeId === parentInodeId && i.parentInodeId !== i.inodeId);
        return inodeList;
    }

    /** Refresh inodes from the server that are at or under {@link path}. */
    async refresh(path: string = ''): Promise<void> {
        await fetchInodeList({ organizationId: this._organization.organizationId, path })
            .then(inodes => this._state.value = inodes.map(x => new InodeModel(x)))
            .catch(reason => log.error('Suppressed error fetching inode list.', reason));
    }
}
