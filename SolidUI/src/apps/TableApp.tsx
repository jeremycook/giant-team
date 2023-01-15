import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export function TableApp() {
    return <>Table</>
}

export const TableAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Table',
    component: TableApp,
}

export default TableAppInfo;