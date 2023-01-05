import { DatabaseChange } from "./GiantTeam.DatabaseDefinition.Changes.Models";
import { Table, Column, TableIndex } from "./GiantTeam.DatabaseDefinition.Models";

// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface AlterTable {
}

export interface AlterTableInput {
    databaseName: string;
    schemaName: string;
    tableName: string;
    table: Table;
}

export interface ChangeDatabaseInput {
    databaseName: string;
    changes: DatabaseChange[];
}

export interface ChangeDatabaseOutput {
}

export interface CreateTableInput {
    databaseName: string;
    schemaName: string;
    tableName: string;
    columns: Column[];
    indexes: TableIndex[];
}

export interface CreateTableOutput {
}

export interface FetchRecords {
    sql: string | null;
    columns: FetchRecordsColumn[];
    records: any[][];
}

export interface FetchRecordsColumn {
    name: string;
    dataType: string;
    nullable: boolean;
}

export interface FetchRecordsInput {
    verbose: boolean | null;
    database: string;
    schema: string;
    table: string;
    columns: FetchRecordsInputColumn[] | null;
    filters: FetchRecordsInputRangeFilter[] | null;
    skip: number | null;
    take: number | null;
}

export interface FetchRecordsInputColumn {
    name: string;
    position: number | null;
    sort: Sort;
    visible: boolean | null;
}

export interface FetchRecordsInputFilter {
    $type: string;
    column: string;
}

export interface FetchRecordsInputOrder {
    column: string;
    desc: boolean | null;
}

export interface FetchRecordsInputRangeFilter extends FetchRecordsInputFilter {
    $type: 'FetchRecordsInputRangeFilter';
    lowerValue: string;
    upperValue: string;
    column: string;
}

export interface FetchWorkspaceInput {
    workspaceName: string | null;
}

export interface ImportDataInput {
    database: string;
    schema: string | null;
    table: string | null;
    createTableIfNotExists: boolean | null;
    data: string | null;
}

export interface ImportDataOutput {
    schema: string;
    table: string;
}

export enum Sort {
    Unsorted = 0,
    Asc = 1,
    Desc = 2,
}