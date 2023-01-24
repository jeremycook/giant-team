import { postJson, DataResponse } from "../helpers/httpHelpers";
import { Inode, OrganizationDetails } from "./GiantTeam.Organization.Etc.Models";
import { AlterDatabaseInput, AlterDatabaseOutput, CreateSpaceInput, FetchRecordsInput, FetchRecords, ImportDataInput, ImportDataOutput, FetchOrganizationDetailsInput, FetchInodeInput, CreateFolderInput, QueryOrganizationInput, FetchInodeByPathInput, FetchInodeChildrenInput, CreateTableInput, FetchInodeListInput } from "./GiantTeam.Organization.Services";
import { QueryTable } from "./GiantTeam.Postgres.Models";

type IFormFileCollection = never;

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface UploadInput {
    organizationId: string;
    path: string;
    files: IFormFileCollection;
}

export interface UploadResult {
    uploadedInodes: Inode[];
}

export const postAlterDatabase = async (input: AlterDatabaseInput) =>
    await postJson('/api/organization/alter-database', input) as DataResponse<AlterDatabaseOutput>;

export const postCreateFolder = async (input: CreateFolderInput) =>
    await postJson('/api/organization/create-folder', input) as DataResponse<Inode>;

export const postCreateSpace = async (input: CreateSpaceInput) =>
    await postJson('/api/organization/create-space', input) as DataResponse<Inode>;

export const postCreateTable = async (input: CreateTableInput) =>
    await postJson('/api/organization/create-table', input) as DataResponse<Inode>;

export const postFetchInodeByPath = async (input: FetchInodeByPathInput) =>
    await postJson('/api/organization/fetch-inode-by-path', input) as DataResponse<Inode>;

export const postFetchInodeChildren = async (input: FetchInodeChildrenInput) =>
    await postJson('/api/organization/fetch-inode-children', input) as DataResponse<Inode[]>;

export const postFetchInode = async (input: FetchInodeInput) =>
    await postJson('/api/organization/fetch-inode', input) as DataResponse<Inode>;

export const postFetchInodeList = async (input: FetchInodeListInput) =>
    await postJson('/api/organization/fetch-inode-list', input) as DataResponse<Inode[]>;

export const postFetchOrganizationDetails = async (input: FetchOrganizationDetailsInput) =>
    await postJson('/api/organization/fetch-organization-details', input) as DataResponse<OrganizationDetails>;

export const postFetchRecords = async (input: FetchRecordsInput) =>
    await postJson('/api/organization/fetch-records', input) as DataResponse<FetchRecords>;

export const postImportData = async (input: ImportDataInput) =>
    await postJson('/api/organization/import-data', input) as DataResponse<ImportDataOutput>;

export const postQueryOrganization = async (input: QueryOrganizationInput) =>
    await postJson('/api/organization/query-organization', input) as DataResponse<QueryTable>;

export const postUpload = async () =>
    await postJson('/api/organization/upload') as DataResponse<UploadResult>;