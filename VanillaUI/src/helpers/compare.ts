
/** Compare things. */
export const compare = Object.freeze({
    /** Shallow compare of two objects for property equality. */
    same: <T extends { [k: string]: any }>(left: T, right: T) => {
        const keys = Object.keys(left);
        if (keys.length !== Object.keys(right).length) {
            return false;
        }

        return keys.every(key => {
            const l = left[key];
            const r = right[key];
            return l === r;
        });
    },
});
