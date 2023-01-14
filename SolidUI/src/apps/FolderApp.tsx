import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export default FolderAppInfo;

export const FolderAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Folder',
    component: FolderApp,
}

export function FolderApp() {
    return <>Folder</>
}