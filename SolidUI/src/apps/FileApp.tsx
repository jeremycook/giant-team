import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export default FileAppInfo;

export const FileAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'File',
    component: FileApp,
}

export function FileApp() {
    return <>File</>
}