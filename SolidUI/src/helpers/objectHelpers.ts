import { camelCase } from "./textHelpers";

/** Returns the value by either executing the function or returning the value directly. */
export function reveal<T>(value: T | (() => T)) {
    return typeof value === 'function' ? (value as () => T)() : value;
}

interface TabularData {
    columns: string[];
    rows: any[][];
}

export function convertTabularDataToObjects<T>(data: TabularData) {
    const columnMap = data.columns
        .reduce((obj, name, i) => ([...obj, {
            name: camelCase(name),
            index: i
        }]), [] as { name: string, index: number }[]);

    return data.rows.map(rec =>
        columnMap.reduce((obj, col) => ({
            ...obj,
            [col.name]: rec[col.index],
        }), {}) as T
    );
}