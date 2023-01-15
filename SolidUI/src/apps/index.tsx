import { Component } from "solid-js";
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models";
import { OrganizationDetails } from "../pages/organization/OrganizationDetailsResource";
import FileAppInfo from "./FileApp";
import FolderAppInfo from "./FolderApp";
import SpaceAppInfo from "./SpaceApp";
import TableAppInfo from "./TableApp";

export interface AppProps {
    organization: OrganizationDetails;
    inode: Inode;
}

export interface AppInfo {
    canHandle(inode: Inode): boolean,
    component: Component<AppProps>
}

export const apps: { [appId: string]: AppInfo } = {
    'File': FileAppInfo,
    'Folder': FolderAppInfo,
    'Space': SpaceAppInfo,
    'Table': TableAppInfo,
}