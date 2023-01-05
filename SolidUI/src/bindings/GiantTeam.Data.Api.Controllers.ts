import { postJson, DataResponse } from "../helpers/httpHelpers";
import { QueryDatabaseProps } from "./GiantTeam.Databases.Database.Services";
import { CreateOrganizationProps, CreateOrganizationResult } from "./GiantTeam.Organizations.Services";
import { QueryTable } from "./GiantTeam.Postgres.Models";
import { CreateWorkspaceRoleInput, CreateWorkspaceRoleOutput } from "./GiantTeam.UserManagement.Services";
import { CreateWorkspaceInput, CreateWorkspaceOutput } from "./GiantTeam.WorkspaceAdministration.Services";
import { Workspace } from "./GiantTeam.Workspaces.Models";
import { AlterTableInput, AlterTable, ChangeDatabaseInput, ChangeDatabaseOutput, CreateTableInput, CreateTableOutput, FetchRecordsInput, FetchRecords, FetchWorkspaceInput, ImportDataInput, ImportDataOutput } from "./GiantTeam.Workspaces.Services";

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export const postAlterTable = async (input: AlterTableInput) =>
    await postJson('/api/alter-table', input) as DataResponse<AlterTable>;

export const postChangeDatabase = async (input: ChangeDatabaseInput) =>
    await postJson('/api/change-database', input) as DataResponse<ChangeDatabaseOutput>;

export const postCreateOrganization = async (input: CreateOrganizationInput) =>
    await postJson('/api/create-organization', input) as DataResponse<CreateOrganizationResult>;

export const postCreateTable = async (input: CreateTableInput) =>
    await postJson('/api/create-table', input) as DataResponse<CreateTableOutput>;

export const postCreateWorkspace = async (input: CreateWorkspaceInput) =>
    await postJson('/api/create-workspace', input) as DataResponse<CreateWorkspaceOutput>;

export const postCreateWorkspaceRole = async (input: CreateWorkspaceRoleInput) =>
    await postJson('/api/create-workspace-role', input) as DataResponse<CreateWorkspaceRoleOutput>;

export const postFetchRecords = async (input: FetchRecordsInput) =>
    await postJson('/api/fetch-records', input) as DataResponse<FetchRecords>;

export const postFetchWorkspace = async (input: FetchWorkspaceInput) =>
    await postJson('/api/fetch-workspace', input) as DataResponse<Workspace>;

export const postImportData = async (input: ImportDataInput) =>
    await postJson('/api/import-data', input) as DataResponse<ImportDataOutput>;

export const postQueryDatabase = async (input: QueryDatabaseInput) =>
    await postJson('/api/query-database', input) as DataResponse<QueryTable>;