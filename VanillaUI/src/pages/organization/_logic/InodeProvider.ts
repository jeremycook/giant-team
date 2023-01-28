import { postFetchInodeList } from '../../../bindings/Organization.Api.Controllers';
import { OrganizationDetails, Inode } from '../../../bindings/Organization.Etc.Models';
import { log } from '../../../helpers/log';


export class InodeProvider {
    public organization: OrganizationDetails;
    public inodes: Inode[];

    constructor(
        props: {
            organization: OrganizationDetails;
            inodes: Inode[];
        }) {
        this.organization = props.organization;
        this.inodes = props.inodes;
    }

    get root() {
        return this.getInode('');
    }

    getInode(path: string) {
        const inode = this.inodes.find(i => i.path === path);
        if (!inode)
            throw Error('Inode not found at ' + path);
        return inode;
    }

    getDirectoryContents(path: string): readonly Inode[] {
        const parentInodeId = this.getInode(path).inodeId;
        const inodeList = this.inodes.filter(i => i.parentInodeId === parentInodeId && i.parentInodeId !== i.inodeId);
        return inodeList;
    }

    async refresh(path: string = '') {
        const response = await postFetchInodeList({
            organizationId: this.organization.organizationId,
            path: path,
        });

        if (!response.ok) {
            // TODO: Notify the user?
            log.warn('InodeProvider HTTP response not ok for {organizationId}.', this.organization.organizationId);
            return;
        }

        this.inodes = response.data;
        // TODO: DISPATCH EVENT
    }
}
