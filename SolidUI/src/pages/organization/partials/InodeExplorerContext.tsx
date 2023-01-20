import { batch, createContext, ParentProps, useContext } from "solid-js";
import { createMutable } from "solid-js/store";
import { postFetchInodeByPath, postFetchInodeChildren } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { mutate } from "../../../helpers/mutate";

export interface ExplorerInode extends Inode {
    children?: ExplorerInode[],
};

export class InodeExplorer {
    private _root: ExplorerInode;

    constructor(public organinzationId: string, rootInode: ExplorerInode) {
        this._root = createMutable(rootInode);
    }

    get root() {
        return this._root;
    }

    // find(path: string) {
    //     // const inode = this._context.find(x => x.path === path);
    //     // return inode;
    // }

    async refresh(path: string) {
        const response = await postFetchInodeByPath({
            organizationId: this.organinzationId,
            path: path,
        });

        if (!response.ok) {
            // TODO: Log something? Notify the user?
            return;
        }

        await batch(async () => {
            if (path === '') {
                this._root.name = response.data.name;

                const childrenResponse = await postFetchInodeChildren({
                    organizationId: this.organinzationId,
                    parentInodeId: response.data.inodeId,
                });

                if (!childrenResponse.ok) {
                    // TODO: Log something? Notify the user?
                    return;
                }

                this._root.children = childrenResponse.data;
            }
            else {
                let parent = this._root;
                const segments = path.split('/');
                for (let i = 0; i < segments.length; i++) {
                    const seg = segments[i];

                    // Make sure we have an array that we can work with
                    // TODO: Some inodes will never have children, decide if this is a good idea
                    parent.children ??= [];

                    const fetchedChild = await this._fetchInode(segments.slice(0, i).join('/'));

                    if (!fetchedChild) {
                        // Inode not found, it may have been deleted or the path is invalid
                        // Make sure it is removed from the tree
                        mutate.removeSome(parent.children, x => x.uglyName === seg);
                        return;
                    }

                    // Try to find the child
                    const childIndex = parent.children.findIndex(x => x.uglyName == seg);

                    if (childIndex < 0) {
                        // Child is missing from the tree, add it
                        parent.children.push(fetchedChild);
                        parent.children.sort((a, b) => a.name.localeCompare(b.name));
                    }
                    else if (i === segments.length - 1) {
                        // Update the child, if it is the one targetted for refresh
                        // TODO: Granular merge so as not to unnecessarily disturb descendants?
                        parent.children[childIndex] = fetchedChild;
                    }
                }
            }
        })
    }

    private async _fetchInode(path: string) {
        const response = await postFetchInodeByPath({
            organizationId: this.organinzationId,
            path: path,
        });
        return response.ok ? response.data : undefined;
    }
}

export const InodeExplorerContext = createContext(new InodeExplorer(undefined!, undefined!));

export function useInodeExplorerContext() { return useContext(InodeExplorerContext); }

export function InodeExplorerProvider(props: { organizationId: string, rootInode: ExplorerInode } & ParentProps) {
    return (
        <InodeExplorerContext.Provider value={new InodeExplorer(props.organizationId, props.rootInode)}>
            {props.children}
        </InodeExplorerContext.Provider>
    );
}
