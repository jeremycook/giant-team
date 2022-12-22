/** Removes all `item` from `array`, and returns the number of items removed. */
export function removeItem<T>(array: T[], item: T) {
    let changes = 0;
    let index = array.indexOf(item);
    while (index > -1) {
        changes++;
        array.splice(index, 1);
        index = array.indexOf(item);
    }
    return changes;
}