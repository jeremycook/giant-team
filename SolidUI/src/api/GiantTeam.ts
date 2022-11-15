// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface User {
    userId: string;
    name: string;
    username: string;
    usernameNormalized: string;
    passwordDigest: string;
    email: string;
    emailVerified: boolean;
    created: string;
}

export interface Workspace {
    workspaceId: string;
    workspaceName?: string;
    recycle: boolean;
    created: string;
    ownerId: string;
    owner?: User;
}

export interface DeleteWorkspaceInput {
    workspaceId?: string;
}

export interface DeleteWorkspaceOutput {
    status: DeleteWorkspaceStatus;
    message?: string;
}

export enum DeleteWorkspaceStatus {
    Deleted = 200,
    Problem = 400,
    NotFound = 404,
}

export interface RecycleWorkspaceInput {
    workspaceId?: string;
}

export interface RecycleWorkspaceOutput {
    status: RecycleWorkspaceStatus;
    message?: string;
}

export enum RecycleWorkspaceStatus {
    Recycled = 200,
    Problem = 400,
    NotFound = 404,
}

export interface CreateWorkspaceInput {
    workspaceName: string;
}

export interface CreateWorkspaceOutput {
    workspaceId: string;
}

export interface JoinInput {
    name: string;
    email: string;
    username: string;
    password: string;
}

export interface JoinOutput {
    userId: string;
}