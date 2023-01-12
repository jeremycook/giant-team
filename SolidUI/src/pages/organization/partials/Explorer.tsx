export function Explorer(props: { organizationId: string, path: string }) {
    return <>
        You are here: {props.organizationId}/{props.path}.
    </>
}