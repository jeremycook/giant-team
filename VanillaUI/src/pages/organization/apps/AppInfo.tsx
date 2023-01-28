import { Inode } from '../../../bindings/Organization.Etc.Models';
import { AppProps } from './AppProps';

export interface AppInfo {
    name: string;
    showInAppDrawer?(inode: Inode): boolean;
    canHandle(inode: Inode): boolean;
    component: (props: AppProps) => JSX.Element;
}
