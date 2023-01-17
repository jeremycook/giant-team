import { Inode, InodeTypeId } from "../bindings/GiantTeam.Organization.Etc.Models"
import { AppInfo } from "./AppInfo";

export function TableApp() {
    return <>Table</>
}

export const TableAppInfo: AppInfo = {
    name: 'Table',
    component: TableApp,
    canHandle: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Table,
    showInAppDrawer: (inode: Inode) => inode.inodeTypeId === InodeTypeId.Folder || inode.inodeTypeId === InodeTypeId.Space,
}

export default TableAppInfo;