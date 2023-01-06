import { postJson, DataResponse } from "../helpers/httpHelpers";
import { QueryDatabaseInput } from "./GiantTeam.Databases.Database.Services";
import { Organization } from "./GiantTeam.Organizations.Directory.Models";
import { FetchOrganizationInput } from "./GiantTeam.Organizations.Directory.Services";
import { CreateSpaceInput, CreateSpaceResult } from "./GiantTeam.Organizations.Organization.Services";
import { CreateOrganizationInput, CreateOrganizationResult } from "./GiantTeam.Organizations.Services";
import { QueryTable } from "./GiantTeam.Postgres.Models";
import { CreateWorkspaceRoleInput, CreateWorkspaceRoleOutput } from "./GiantTeam.UserManagement.Services";
import { ChangeDatabaseInput, ChangeDatabaseOutput, FetchRecordsInput, FetchRecords, ImportDataInput, ImportDataOutput } from "./GiantTeam.Workspaces.Services";

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export const postChangeDatabase = async (input: ChangeDatabaseInput) =>
    await postJson('/api/change-database', input) as DataResponse<ChangeDatabaseOutput>;

export const postCreateOrganization = async (input: CreateOrganizationInput) =>
    await postJson('/api/create-organization', input) as DataResponse<CreateOrganizationResult>;

export const postCreateSpace = async (input: CreateSpaceInput) =>
    await postJson('/api/create-space', input) as DataResponse<CreateSpaceResult>;

export const postCreateWorkspaceRole = async (input: CreateWorkspaceRoleInput) =>
    await postJson('/api/create-workspace-role', input) as DataResponse<CreateWorkspaceRoleOutput>;

export const postFetchOrganization = async (input: FetchOrganizationInput) =>
    await postJson('/api/fetch-organization', input) as DataResponse<Organization>;

export const postFetchRecords = async (input: FetchRecordsInput) =>
    await postJson('/api/fetch-records', input) as DataResponse<FetchRecords>;

export const postImportData = async (input: ImportDataInput) =>
    await postJson('/api/import-data', input) as DataResponse<ImportDataOutput>;

export const postQueryDatabase = async (input: QueryDatabaseInput) =>
    await postJson('/api/query-database', input) as DataResponse<QueryTable>;