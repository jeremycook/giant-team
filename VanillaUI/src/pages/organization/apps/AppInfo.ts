import { Inode } from '../../../bindings/Organization.Etc.Models';
import { BaseNode } from '../../../helpers/h';
import { AppProps } from './AppProps';

export interface AppInfo {
    name: string;
    showInAppDrawer?(inode: Inode): boolean;
    canHandle(inode: Inode): boolean;
    component: (props: AppProps) => BaseNode;
}
