// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface Organization {
    organizationId: string;
    name: string;
    databaseName: string;
    created: Date;
    roles: OrganizationRole[] | null;
}

export interface OrganizationRole {
    organizationRoleId: string;
    created: Date;
    organizationId: string;
    organization: Organization | null;
    name: string;
    dbRole: string;
    description: string;
}

export interface User {
    userId: string;
    name: string;
    username: string;
    dbUser: string;
    email: string;
    emailVerified: boolean;
    created: Date;
}