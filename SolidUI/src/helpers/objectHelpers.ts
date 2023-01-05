import { camelCase } from "./textHelpers";

/** Returns the value by either executing the function or returning the value directly. */
export function reveal<T>(value: T | (() => T)) {
    return typeof value === 'function' ? (value as () => T)() : value;
}

interface TabularData {
    columns: string[];
    rows: any[][];
}

export function objectifyTabularData<T>(data: TabularData) {
    const columnMap = data.columns
        .reduce((obj, name, i) => ([...obj, {
            name: camelCase(name),
            index: i
        }]), [] as { name: string, index: number }[]);

    return data.rows.map(rec =>
        columnMap.reduce((obj, col) => ({
            ...obj,
            [col.name]: parseValue(rec[col.index]),
        }), {}) as T
    );
}

const isoRegex = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;

/** Converts ISO date strings into Date objects. Leaves other values as-is. */
export function parseValue(value: string | number | null) {
    if (typeof value === 'string') {
        if (isoRegex.test(value))
            return new Date(value);
    }
    return value;
}

/** Revive JSON with {@link parseValue}.
 * @example JSON.parse(someJson, jsonParseValueReviver)
 */
export function parseValueJsonReviver(this: any, key: string, value: any) {
    return parseValue(value);
}

/** Parse JSON with the {@link parseValueJsonReviver}. */
export function parseJson(json: string) {
    return JSON.parse(json, parseValueJsonReviver);
}