// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface ObjectStatus {
    status: number;
    statusText: string;
    message: string;
    details: ObjectStatusDetail[];
}

export interface ObjectStatusDetail {
    message: string;
    members: string[];
}

export interface Column {
    position: number;
    name: string;
    storeType: string;
    isNullable: boolean;
    defaultValueSql: string | null;
    computedColumnSql: string | null;
}

export interface Database {
    defaultSchema: string | null;
    schemas: Schema[];
    scripts: string[];
}

export interface DefaultPrivileges {
    privileges: string;
    objectType: string;
    grantee: string;
}

export enum DefaultPrivilegesEnum {
    Tables = 0,
    Sequences = 1,
    Functions = 2,
    Types = 3,
}

export interface Schema {
    name: string;
    owner: string | null;
    tables: Table[];
    privileges: SchemaPrivileges[];
    defaultPrivileges: DefaultPrivileges[];
}

export interface SchemaPrivileges {
    grantee: string;
    privileges: string;
}

export const StoreType = {
    array: 'array',
    bigint: 'bigint',
    bigIntMultirange: 'bigintmultirange',
    bigIntRange: 'bigintrange',
    bit: 'bit',
    boolean: 'boolean',
    box: 'box',
    bytea: 'bytea',
    char: 'char',
    cid: 'cid',
    cidr: 'cidr',
    circle: 'circle',
    citext: 'citext',
    date: 'date',
    dateMultirange: 'datemultirange',
    dateRange: 'daterange',
    double: 'double',
    geography: 'geography',
    geometry: 'geometry',
    hstore: 'hstore',
    inet: 'inet',
    int2Vector: 'int2vector',
    integer: 'integer',
    integerMultirange: 'integermultirange',
    integerRange: 'integerrange',
    internalChar: 'internalchar',
    interval: 'interval',
    json: 'json',
    jsonb: 'jsonb',
    jsonPath: 'jsonpath',
    line: 'line',
    lQuery: 'lquery',
    lSeg: 'lseg',
    lTree: 'ltree',
    lTxtQuery: 'ltxtquery',
    macAddr: 'macaddr',
    macAddr8: 'macaddr8',
    money: 'money',
    multirange: 'multirange',
    name: 'name',
    numeric: 'numeric',
    numericMultirange: 'numericmultirange',
    numericRange: 'numericrange',
    oid: 'oid',
    oidvector: 'oidvector',
    path: 'path',
    pgLsn: 'pglsn',
    point: 'point',
    polygon: 'polygon',
    range: 'range',
    real: 'real',
    refcursor: 'refcursor',
    regconfig: 'regconfig',
    regtype: 'regtype',
    smallint: 'smallint',
    text: 'text',
    tid: 'tid',
    time: 'time',
    timestamp: 'timestamp',
    timestampMultirange: 'timestampmultirange',
    timestampRange: 'timestamprange',
    timestampTz: 'timestamptz',
    timestampTzMultirange: 'timestamptzmultirange',
    timestampTzRange: 'timestamptzrange',
    timeTz: 'timetz',
    tsQuery: 'tsquery',
    tsVector: 'tsvector',
    unknown: 'unknown',
    uuid: 'uuid',
    varbit: 'varbit',
    varchar: 'varchar',
    xid: 'xid',
    xid8: 'xid8',
    xml: 'xml',
}

export interface Table {
    name: string;
    owner: string | null;
    columns: Column[];
    indexes: TableIndex[];
}

export interface TableIndex {
    name: string | null;
    indexType: TableIndexType;
    columns: string[];
}

export enum TableIndexType {
    Index = 0,
    UniqueConstraint = 1,
    PrimaryKey = 2,
}

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
    dbRole: DbRole | null;
}

export interface CreateRoleInput {
    workspaceName: string | null;
    roleName: string | null;
}

export interface CreateRoleOutput {
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

export interface SessionUser {
    userId: string;
    sub: string;
    username: string;
    dbRole: string;
    dbLogin: string;
    dbPassword: string;
}

export interface VerifyPasswordInput {
    username: string;
    password: string;
}

export interface VerifyPasswordOutput {
    userId: string;
}

export interface CreateWorkspaceInput {
    workspaceName: string | null;
    isPublic: boolean;
}

export interface CreateWorkspaceOutput {
}

export interface DeleteWorkspaceInput {
    workspaceId: string | null;
}

export interface DeleteWorkspaceOutput {
    status: DeleteWorkspaceStatus;
    message: string | null;
}

export enum DeleteWorkspaceStatus {
    Success = 200,
    Problem = 400,
}

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

export interface Workspace {
    name: string;
    owner: string;
    zones: Schema[];
}

export interface AlterTable {
}

export interface AlterTableInput {
    databaseName: string;
    schemaName: string;
    tableName: string;
    table: Table;
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
    discriminator: string;
    column: string;
}

export interface FetchRecordsInputOrder {
    column: string;
    desc: boolean | null;
}

export interface FetchRecordsInputRangeFilter {
    lowerValue: string;
    upperValue: string;
    discriminator: string;
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