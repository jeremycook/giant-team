import { Accessor, Setter } from "solid-js";
import { postFetchInodeList } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, OrganizationDetails } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { log } from "../../../helpers/logging";


export class InodeProvider {
    public organization: OrganizationDetails;
    public inodes: Accessor<Inode[]>;
    public setInodes: Setter<Inode[]>;

    constructor(
        props: {
            organization: OrganizationDetails;
            inodes: Accessor<Inode[]>;
            setInodes: Setter<Inode[]>;
        }) {
        this.organization = props.organization;
        this.inodes = props.inodes;
        this.setInodes = props.setInodes;
    }

    get root() {
        return this.getInode('');
    }

    getInode(path: string) {
        const inode = this.inodes().find(i => i.path === path);
        if (!inode)
            throw Error('Inode not found at ' + path);
        return inode;
    }

    getDirectoryContents(path: string): readonly Inode[] {
        const parentInodeId = this.getInode(path).inodeId;
        const inodeList = this.inodes().filter(i => i.parentInodeId === parentInodeId && i.parentInodeId !== i.inodeId);
        return inodeList;
    }

    async refresh(path: string = '') {
        const response = await postFetchInodeList({
            organizationId: this.organization.organizationId,
            path: path,
        });

        if (!response.ok) {
            // TODO: Notify the user?
            log.warn('InodeExplorer.refreshLikePath HTTP response not ok for {organizationId}.', [this.organization.organizationId]);
            return;
        }

        this.setInodes(response.data);
    }
}
