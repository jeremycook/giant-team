
/** Immutable array helpers. */
export const immute = Object.freeze({
    removeAt: <T>(array: ReadonlyArray<T>, index: number) => {
        return array.filter((_, i) => i !== index);
    },

    remove: <T>(array: ReadonlyArray<T>, item: T) => {
        return array.filter(x => x !== item);
    },

    removeSome: <T>(array: ReadonlyArray<T>, predicate: (item: T) => boolean) => {
        return array.filter(x => !predicate(x));
    },

    replace: <T>(array: ReadonlyArray<T>, oldItem: T, item: T) => {
        const copy = [...array];
        const index = copy.findIndex(x => x === oldItem);
        copy.splice(index, 1, item);
        return copy as ReadonlyArray<T>;
    },
});
