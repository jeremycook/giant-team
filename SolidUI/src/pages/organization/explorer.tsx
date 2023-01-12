import { Show } from "solid-js";
import { useOrganizationRouteData } from "./organization";
import { Explorer } from "./partials/Explorer";

export default function ExplorerPage() {
    const org = useOrganizationRouteData();

    return <>
        <Show when={org.data}>{() => {
            return <>
                <h1>{org.data!.name}</h1>
                <Explorer organizationId={org.data!.organizationId} datum={org.data!.rootDatum} />
            </>
        }}</Show>
    </>
}