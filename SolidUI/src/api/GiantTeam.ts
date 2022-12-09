// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface DbRole {
    roleId: string;
    created: Date;
}

export interface User {
    userId: string;
    name: string;
    username: string;
    invariantUsername: string;
    email: string;
    emailVerified: boolean;
    created: Date;
    dbRoleId: string;
    dbRole?: DbRole;
}

export interface CreateTeamInput {
    teamName?: string;
}

export interface CreateTeamOutput {
    teamId: string;
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

export interface VerifyPasswordInput {
    username: string;
    password: string;
}

export interface VerifyPasswordOutput {
    userId: string;
}

export interface CreateWorkspaceInput {
    workspaceName?: string;
    workspaceOwner?: string;
}

export interface CreateWorkspaceOutput {
    workspaceName: string;
}

export interface DeleteWorkspaceInput {
    workspaceId?: string;
}

export interface DeleteWorkspaceOutput {
    status: DeleteWorkspaceStatus;
    message?: string;
}

export enum DeleteWorkspaceStatus {
    Success = 200,
    Problem = 400,
}

export interface FetchRoleInput {
    roleName?: string;
}

export interface FetchRoleMemberOutput {
    roleName: string;
    inherit: boolean;
    teamAdmin: boolean;
}

export interface FetchRoleOutput {
    roleName: string;
    canLogin: boolean;
    createDb: boolean;
    inherit: boolean;
    members: FetchRoleMemberOutput[];
}

export interface FetchWorkspaceInput {
    workspaceName?: string;
}

export interface FetchWorkspaceOutput {
    workspaceName: string;
    workspaceOwner: string;
}

export interface FetchRecordsInput {
    verbose?: boolean;
    database: string;
    schema: string;
    table: string;
    columns?: string[];
    filters?: FetchRecordsInputRangeFilter[];
    orderBy?: FetchRecordsInputOrder[];
    skip?: number;
    take?: number;
}

export interface FetchRecordsInputColumn {
    column: string;
    alias?: string;
}

export interface FetchRecordsInputFilter {
    discriminator: string;
    column: string;
}

export interface FetchRecordsInputOrder {
    column: string;
    asc?: boolean;
}

export interface FetchRecordsInputRangeFilter {
    lowerValue: string;
    upperValue: string;
    discriminator: string;
    column: string;
}

export interface FetchRecordsOutput {
    sql?: string;
    columns: FetchRecordsOutputColumn[];
    records: any[][];
}

export interface FetchRecordsOutputColumn {
    name: string;
    dataType: string;
    nullable: boolean;
}