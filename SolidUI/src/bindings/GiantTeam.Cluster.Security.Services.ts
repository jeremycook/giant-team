// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface FetchRoleInput {
    roleName: string | null;
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