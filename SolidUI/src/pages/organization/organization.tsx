import { ProcessOperatorProvider } from "./partials/ProcessOperatorContext";
import { OrganizationDetailsResource } from "./OrganizationDetailsResource";
import { useRouteData } from "@solidjs/router";
import { ShowItem } from "../../widgets/ShowItem";
import { InodeExplorerProvider } from "./partials/InodeExplorerContext";
import { Organization } from "./partials/Organization";
import { Loading } from "../../partials/Loading";
import { createEffect } from "solid-js";
import { OrganizationDetailsProvider } from "./partials/OrganizationDetailsProvider";

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

            <OrganizationDetailsProvider organizationDetails={org}>
                <InodeExplorerProvider organization={org} rootInode={{
                    ...org.rootInode,
                    children: org.rootChildren,
                }}>
                    <ProcessOperatorProvider>
                        <Organization organization={org} />
                    </ProcessOperatorProvider>
                </InodeExplorerProvider>
            </OrganizationDetailsProvider>

        </>}</ShowItem>
    </>
}