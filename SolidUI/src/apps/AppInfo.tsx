import { Component } from "solid-js";
import { Inode } from "../bindings/GiantTeam.Organization.Etc.Models";
import { AppProps } from "./AppProps";

export interface AppInfo {
    name: string;
    showInAppDrawer?(inode: Inode): boolean;
    canHandle(inode: Inode): boolean;
    component: Component<AppProps>;
}
