import { Explorer } from "./partials/Explorer";
import { ProcessOperator } from "./ProcessOperator";
import { InodeResource } from "./InodeResource";
import { OrganizationDetailsResource } from "./OrganizationDetailsResource";
import { useParams, useRouteData } from "@solidjs/router";
import { createEffect, createResource, For, Suspense } from "solid-js";
import { Loading } from "../../partials/Loading";
import { MainLayout } from "../../partials/MainLayout";
import { RenderSection } from "../../partials/Section";
import SectionedLayout from "../../partials/SectionedLayout";
import { postFetchInode } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Dynamic } from "solid-js/web";
import { apps } from "../../apps";
import Dialog from "../../widgets/Dialog";

export default function OrganizationPage() {
    const params = useParams();

    const organization = useRouteData<OrganizationDetailsResource>();
    const processOperator = new ProcessOperator();

    const resourceReturn = createResource(
        () => ({ organizationId: params.organization, path: params.path ?? '' }),
        (props) => postFetchInode(props)
    );
    const inode = new InodeResource(resourceReturn);

    createEffect(() => {
        if (inode.data?.inode.name) {
            document.title = inode.data.inode.name;
        }
    })

    return <SectionedLayout>
        <MainLayout navBarChildren={<RenderSection name='navbar-start' />}>

            <Suspense fallback={<Loading />}>
                <Explorer organization={organization} processOperator={processOperator} inode={inode} />
            </Suspense>

            <For each={processOperator.processes}>{(process, i) => <Dialog
                title={process.app.name + ': ' + process.inode.name}
                onDismiss={() => processOperator.terminateByIndex(i())}
            >
                <Dynamic component={apps[process.app.appId].component} {...{ app: process.app, inode: process.inode }} />
            </Dialog>}</For>

        </MainLayout >
    </SectionedLayout>
}