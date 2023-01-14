
/** Immutable array helpers. */
export const immute = Object.freeze({
    removeAt: <T>(array: ReadonlyArray<T>, index: number) => {
        const y = [...array];
        y.splice(index, 1);
        return y;
    },

    remove: <T>(array: ReadonlyArray<T>, item: T) => {
        return array.filter(x => x !== item);
    },
});
