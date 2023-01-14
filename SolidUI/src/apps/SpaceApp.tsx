import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export default SpaceAppInfo;

export const SpaceAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Space',
    component: SpaceApp,
}

export function SpaceApp() {
    return <>Space</>
}