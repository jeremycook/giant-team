import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export function FolderApp() {
    return <>Folder</>
}

export const FolderAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Folder',
    component: FolderApp,
}

export default FolderAppInfo;