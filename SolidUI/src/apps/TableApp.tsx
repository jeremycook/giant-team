import { AppInfo } from ".";
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models";

export default TableAppInfo;

export const TableAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Table',
    component: TableApp,
}

export function TableApp() {
    return <>Table</>
}