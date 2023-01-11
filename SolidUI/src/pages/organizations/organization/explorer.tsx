import { useParams } from "@solidjs/router";
import { Show } from "solid-js";
import { useOrganizationRouteData } from "../organization";
import { Explorer } from "./partials/Explorer";

export default function ExplorerPage() {
    const route = useParams<{ path: string }>();
    const organization = useOrganizationRouteData();

    return <>
        <Show when={organization.data}>{() => {
            return <>
                <h1>{organization.data!.name}</h1>
                <Explorer organizationId={organization.data!.organizationId} path={route.path} />
            </>
        }}</Show>
    </>
}