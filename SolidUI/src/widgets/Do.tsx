
/** Execute children as an action, discarding the return value, if any. */
export function Do(props: { children: () => any }) {
    return props.children() ? null : null;
}