// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface File {
    inodeId: string;
    contentType: string;
    data: string;
}

export interface Inode {
    inodeId: string;
    parentInodeId: string;
    inodeTypeId: string;
    name: string;
    uglyName: string;
    created: Date | null;
    path: string;
}

export interface InodeAccess {
    roleId: string;
    permissions: PermissionId[];
}

export interface InodeChildConstraint {
    inodeTypeId: string;
}

export enum InodeId {
    Root = '00000000-0000-0000-0000-000000000000',
    Etc = '3e544ebc-f30a-471f-a8ec-f9e3ac84f19a',
}

export interface InodeType {
    inodeTypeId: string;
    allowedChildNodeTypeIds: string[];
    allowedParentNodeTypeIds: string[];
}

export interface InodeTypeConstraint {
    parentInodeTypeId: string;
}

export enum InodeTypeId {
    Root = 'Root',
    Space = 'Space',
    Folder = 'Folder',
    File = 'File',
    Table = 'Table',
}

export interface OrganizationDetails {
    organizationId: string;
    inodeTypes: { [k: string]: InodeType };
    roles: Role[];
    rootInode: Inode;
    rootChildren: Inode[];
}

export enum PermissionId {
    a = 97,
    d = 100,
    m = 109,
    r = 114,
    w = 119,
}

export interface Role {
    roleId: string;
    name: string;
    created: Date;
}

export interface Space {
    inode: Inode;
}