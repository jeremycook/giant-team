import { ProcessOperator, ProcessOperatorContext } from "./partials/ProcessOperatorContext";
import { OrganizationDetailsResource } from "./OrganizationDetailsResource";
import { useRouteData } from "@solidjs/router";
import { ShowItem } from "../../widgets/ShowItem";
import { InodeExplorer, InodeExplorerContext } from "./partials/InodeExplorerContext";
import { Organization } from "./partials/Organization";
import { Loading } from "../../partials/Loading";
import { createEffect } from "solid-js";

export default function OrganizationPage() {
    const organizationResource = useRouteData<OrganizationDetailsResource>();

    createEffect(() => {
        if (organizationResource.data?.rootInode.name) {
            document.title = organizationResource.data.rootInode.name;
        }
    })

    return <>
        <ShowItem when={organizationResource.data} fallback={
            <Loading />
        }>{org => <>

            <InodeExplorerContext.Provider value={new InodeExplorer(org.organizationId, org.rootInode)}>
                <ProcessOperatorContext.Provider value={new ProcessOperator()}>
                    <Organization organization={org} />
                </ProcessOperatorContext.Provider>
            </InodeExplorerContext.Provider>

        </>}</ShowItem>
    </>
}