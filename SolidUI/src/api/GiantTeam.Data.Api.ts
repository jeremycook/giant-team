import {
    CreateTeamInput,
    CreateTeamOutput,
    CreateWorkspaceInput,
    CreateWorkspaceOutput,
    RecycleWorkspaceInput,
    RecycleWorkspaceOutput,
    Workspace
} from "./GiantTeam";

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface GetWorkspaceInput {
    workspaceId?: string;
}

export interface GetWorkspaceOutput {
    status: GetWorkspaceStatus;
    message?: string;
    workspace?: Workspace;
}

export enum GetWorkspaceStatus {
    Success = 200,
    Problem = 400,
    NotFound = 404,
}

export const postCreateTeam = async (input: CreateTeamInput): Promise<CreateTeamOutput> => {
    const response = await fetch("/api/create-team", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(input)
    });
    return response.json();
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