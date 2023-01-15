import { AppInfo } from "."
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models"

export function SpaceApp() {
    return <>Space</>
}

export const SpaceAppInfo: AppInfo = {
    canHandle: (inode: Inode) => inode.inodeTypeId === 'Space',
    component: SpaceApp,
}

export default SpaceAppInfo;