import { RecycleWorkspaceInput, RecycleWorkspaceOutput, Workspace } from "./GiantTeam";

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface CreateWorkspaceInput {
    workspaceName?: string;
}

export interface CreateWorkspaceOutput {
    status: CreateWorkspaceStatus;
    errorMessage?: string;
    workspaceId?: string;
}

export enum CreateWorkspaceStatus {
    Created = 201,
    Error = 400,
}

export interface GetWorkspaceInput {
    workspaceId?: string;
}

export interface GetWorkspaceOutput {
    status: GetWorkspaceStatus;
    message?: string;
    workspace?: Workspace;
}

export enum GetWorkspaceStatus {
    Found = 200,
    Problem = 400,
    NotFound = 404,
}

export const postCreateWorkspace = async (input: CreateWorkspaceInput): Promise<CreateWorkspaceOutput> => {
    const response = await fetch("/api/create-workspace", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
}

export const postGetWorkspace = async (input: GetWorkspaceInput): Promise<GetWorkspaceOutput> => {
    const response = await fetch("/api/get-workspace", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
}

export const postRecycleWorkspace = async (input: RecycleWorkspaceInput): Promise<RecycleWorkspaceOutput> => {
    const response = await fetch("/api/recycle-workspace", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
}