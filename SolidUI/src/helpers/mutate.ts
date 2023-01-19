
/** Mutable array helpers. */
export const mutate = Object.freeze({
    removeAt: <T>(array: Array<T>, index: number) => {
        if (index >= 0)
            array.splice(index, 1);
        return array;
    },

    removeSome: <T>(array: Array<T>, predicate: (item: T) => boolean) => {
        let index = array.findIndex(predicate);
        while (index >= 0) {
            mutate.removeAt(array, index);
            index = array.findIndex(predicate);
        };
        return array;
    },

    remove: <T>(array: Array<T>, item: T) => {
        return mutate.removeSome(array, x => x === item);
    },
});
