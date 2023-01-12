// DO NOT MODIFY BELOW THIS LINE
// Generated by GiantTeam.Tools

export interface Datum {
    typeId: string;
    datumId: string;
    parentId: string;
    name: string;
    created: Date;
    path: string;
    children: Datum[] | null;
}

export const DatumId = {
}

export interface DatumType {
    typeId: string;
    constraints: DatumTypeConstraint[];
}

export interface DatumTypeConstraint {
    parentTypeId: string;
}

export interface File {
    datum: Datum;
    contentType: string;
    data: string;
}

export interface Space {
    datum: Datum;
}