// export function sql(strings: { raw: readonly string[] | ArrayLike<string>; }, ...values: any[]) {
//     return String.raw(strings, ...values);
// }

export interface Query<T> {
    text: string;
    values: any[];
}

export const sql = <T = any>(
    strings: TemplateStringsArray,
    ...values: any[]
): Query<T> => ({
    text: String.raw(strings, ...values.map((_, i) => `$${i + 1}`)).trim(),
    values,
});

export function identifer(name: string) {
    return '"' + name.replaceAll('"', '""') + '"';
}

export function dualIdentifer(name1: string, name2: string) {
    return identifer(name1) + '.' + identifer(name2);
}
