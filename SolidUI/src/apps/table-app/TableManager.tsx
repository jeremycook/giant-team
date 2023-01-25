import { Accessor, createEffect, createResource, createSignal, on, Setter } from "solid-js";
import { createStore, SetStoreFunction } from "solid-js/store";
import { CreateColumn } from "../../bindings/GiantTeam.DatabaseDefinition.Alterations.Models";
import { Column, Schema, Table } from "../../bindings/GiantTeam.DatabaseDefinition.Models";
import { postAlterDatabase, postQuery } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Inode, InodeTypeId, OrganizationDetails } from "../../bindings/GiantTeam.Organization.Etc.Models";
import { DataResource } from "../../helpers/DataResource";
import { parseJson } from "../../helpers/objectHelpers";
import { dualIdentifer as identifer2, identifer } from "../../helpers/sqlHelpers";

export type FieldValue = (string | number | boolean | Date | null);

class TableManagerRow {

    private _selected: Accessor<boolean>;
    setSelected: Setter<boolean>;

    originalRecord: readonly FieldValue[];

    private _values: Accessor<FieldValue[]>;
    setValues: Setter<FieldValue[]>;

    /** Indexes of fields that have changed. */
    private _dirtyValues: Accessor<number[]>;
    private _setDirtyValues: Setter<number[]>;

    constructor(props: {
        selected?: boolean;
        record: FieldValue[];
    }) {
        [this._selected, this.setSelected] = createSignal(props.selected ?? false);
        this.originalRecord = [...props.record];
        [this._values, this.setValues] = createSignal(props.record);
        [this._dirtyValues, this._setDirtyValues] = createSignal([]);

        createEffect(on(this._values, (record) => {
            const dirtyFieldIndexes = record
                .map((v, i) => this.originalRecord[i] !== v ? i : -1)
                .filter(i => i > -1);

            this._setDirtyValues(dirtyFieldIndexes);
        }, { defer: true }));
    }

    get selected() {
        return this._selected();
    }

    get values() {
        return this._values();
    }

    get dirtyValueIndexes() {
        return this._dirtyValues();
    }
}

class TableManagerData {
    columns: readonly string[];

    private _rows: Accessor<TableManagerRow[]>;
    setRows: Setter<TableManagerRow[]>;
    get rows() {
        return this._rows();
    }

    private _dirty: Accessor<number[]>;
    private _setDirty: Setter<number[]>;
    /** Indexes of dirty rows. */
    get dirtyRowIndexes() {
        return this._dirty();
    }

    constructor(props: {
        columns: string[];
        rows: FieldValue[][];
    }) {
        this.columns = props.columns;
        [this._rows, this.setRows] = createSignal(props.rows.map(r => new TableManagerRow({
            record: r,
        })));
        [this._dirty, this._setDirty] = createSignal([]);

        createEffect(() => {
            const dirty = this._rows()
                .map((r, i) => r.dirtyValueIndexes.includes(i) ? i : -1)
                .filter(i => i > -1);

            this._setDirty(dirty);
        });
    }
}

export class TableManager {
    private _organization: OrganizationDetails;

    private _activities: Accessor<(CreateColumn)[]>;
    private _setActivities: Setter<(CreateColumn)[]>;

    private _data: Accessor<TableManagerData>;
    private _setData: Setter<TableManagerData>;

    private _definitionResource: DataResource<{
        databaseName: string;
        databaseOwner: string;
        schemas: readonly Schema[];
        schemaName: string;
        table: Table;
        setTable: SetStoreFunction<Table>;
    } | undefined, unknown>;

    constructor(props: {
        organization: OrganizationDetails;
        inode: Inode;
    }) {
        if (props.inode.inodeTypeId !== InodeTypeId.Table) {
            throw new Error('The inodeTypeId of the inode property must be a ' + InodeTypeId.Table + '.');
        }

        this._organization = props.organization;
        [this._activities, this._setActivities] = createSignal([]);
        [this._data, this._setData] = createSignal(new TableManagerData({ columns: [], rows: [] }));

        this._definitionResource = new DataResource(createResource(
            () => ({
                organizationId: props.organization.organizationId,
                schemaName: props.inode.path.split('/')[0],
                tableName: props.inode.uglyName,
            }),
            async (props) => {
                const response = await postQuery({ organizationId: props.organizationId, sql: 'SELECT name, owner, schemas FROM etc.database_definition' });
                if (!response.ok) return undefined;

                const data = response.data;
                const row = data.rows[0];

                const schemas = parseJson(row[2]) as ReadonlyArray<Schema>;

                const tableData = schemas
                    .find(s => s.name === props.schemaName)!
                    .tables
                    .find(t => t.name === props.tableName)!;

                const [table, setTable] = createStore(tableData);

                return {
                    databaseName: row[0] as string,
                    databaseOwner: row[1] as string,
                    schemas: schemas,
                    schemaName: props.schemaName,
                    table,
                    setTable,
                };
            }
        ));

        createEffect(async () => {
            if (!this._definitionResource.data) return;

            const table = this.table;
            const columnNames = table.columns.map(c => c.name);

            const response = await postQuery({
                organizationId: props.organization.organizationId,
                sql: `SELECT ${columnNames.map(identifer).join(',')} FROM ${identifer2(this.schemaName, table.name)}`
            });
            if (!response.ok) return;

            this._setData(new TableManagerData({
                columns: response.data.columns,
                rows: response.data.rows,
            }));
        });

        createEffect(async () => {
            const processing = [...this._activities()];
            if (processing.length <= 0) return;

            const response = await postAlterDatabase({
                organizationId: this._organization.organizationId,
                changes: processing,
            });
            if (!response.ok) throw new Error(response.message);

            // Remove processed activities
            this._setActivities(prev => prev.filter(x => !processing.includes(x)));
            // Refetch
            await this._definitionResource.refetch();
        }, null, { name: 'TableManager activity processor' });
    }

    get definition() {
        return this._definitionResource.data;
    }

    get schemaName() {
        const schemaName = this.definition?.schemaName;
        if (!schemaName)
            throw new Error('The database definition has not loaded.');
        return schemaName;
    }

    get table() {
        const table = this.definition?.table;
        if (!table)
            throw new Error('The database definition has not loaded.');
        return table;
    }

    get data() {
        return this._data();
    }

    /**
     * Appends an empty row without trying to insert it into the database.
     * Insertion is based on dirty status.
     */
    appendEmptyRow(): void {
        const data = this.data;
        data.setRows([...data.rows, new TableManagerRow({ record: data.columns.map(c => null) })])
    }

    createColumn(column: Column) {
        const createColumn: CreateColumn = {
            $type: 'CreateColumn',
            schemaName: this.schemaName,
            tableName: this.table.name,
            column: column,
        };

        this._setActivities(prev => [...prev, createColumn]);
    }
}
