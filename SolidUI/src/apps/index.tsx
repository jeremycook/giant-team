import { AppInfo } from "./AppInfo";
import FileAppInfo from "./FileApp";
import FolderAppInfo from "./FolderApp";
import SpaceAppInfo from "./SpaceApp";
import TableAppInfo from "./table-app/TableApp";

export const apps: AppInfo[] = [
    FileAppInfo,
    FolderAppInfo,
    SpaceAppInfo,
    TableAppInfo,
]