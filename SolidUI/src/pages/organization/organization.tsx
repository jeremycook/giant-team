import { ProcessOperator } from "./ProcessOperator";
import { InodeResource } from "./InodeResource";
import { OrganizationDetails, OrganizationDetailsResource } from "./OrganizationDetailsResource";
import { useParams, useRouteData } from "@solidjs/router";
import { createEffect, createResource, createSignal, For, Show, Suspense } from "solid-js";
import { Loading } from "../../partials/Loading";
import { MainLayout } from "../../partials/MainLayout";
import { RenderSection } from "../../partials/Section";
import SectionedLayout from "../../partials/SectionedLayout";
import { postFetchInode, postFetchInodeByPath } from "../../bindings/GiantTeam.Organization.Api.Controllers";
import { Dynamic } from "solid-js/web";
import { apps } from "../../apps";
import { ShowItem } from "../../widgets/ShowItem";
import { Inode, InodeId } from "../../bindings/GiantTeam.Organization.Etc.Models";
import Dialog, { DialogAnchor } from "../../widgets/Dialog";
import { AddOutlineIcon, DismissOutlineIcon } from "../../partials/Icons";
import { getScreenCenter } from "../../helpers/htmlHelpers";
import { InodeNavigator, InodeTree } from "./partials/InodeTree";

export function AppDrawer(props: {
    org: OrganizationDetails,
    inode: Inode,
    processOperator: ProcessOperator,
    onLaunched: (e: MouseEvent & {
        currentTarget: HTMLButtonElement;
        target: Element;
    }) => void,
}) {
    return <>
        <div class='grid grid-cols-3 gap-1 w-300px'>
            <For each={apps}>{app => <>
                <Show when={app.showInAppDrawer ? app.showInAppDrawer(props.inode) : true}>
                    <button type='button' class='card text-center b b-solid'
                        onclick={(e) => {
                            props.processOperator.launch(app, props.org, props.inode);
                            props.onLaunched(e);
                        }}>
                        {app.name}
                    </button>
                </Show>
            </>}</For>
        </div>
    </>
}

export default function OrganizationPage() {
    const params = useParams();

    const organizationResource = useRouteData<OrganizationDetailsResource>();
    const [inodeNavigatorResource] = createResource(
        () => ({ organizationId: params.organization, inodeId: InodeId.Root }),
        async (props) => {
            const inode = await postFetchInode(props);
            if (inode.ok) {
                return new InodeNavigator(inode.data);
            }
            else {
                return undefined;
            }
        }
    );
    const inodeResource = new InodeResource(createResource(
        () => ({ organizationId: params.organization, path: params.path ?? '' }),
        (props) => postFetchInodeByPath(props)
    ));
    const processOperator = new ProcessOperator();

    const [showAppDrawer, setShowAppDrawer] = createSignal(false);

    createEffect(() => {
        if (inodeResource.data?.name) {
            document.title = inodeResource.data.name;
        }
    })

    let loadedProcesses = false;
    createEffect(() => {
        if (loadedProcesses) return;

        const org = organizationResource.data,
            inode = inodeResource.data;

        if (org && inode) {
            loadedProcesses = true;
            apps.forEach(app => processOperator.launch(app, org, inode))
        }
    });

    return <SectionedLayout>
        <MainLayout navBarChildren={<RenderSection name='navbar-start' />}>

            <div class='flex gap-4 h-100%'>
                <div class='overflow-auto w-200px'>

                    <Suspense fallback={<Loading />}>
                        <ShowItem when={organizationResource.data}>{org =>
                            <ShowItem when={inodeNavigatorResource()}>{navigator =>
                                <InodeTree
                                    organization={org}
                                    processOperator={processOperator}
                                    navigator={navigator}
                                />
                            }</ShowItem>
                        }</ShowItem>
                    </Suspense>

                </div>
                <div class='grow flex flex-col'>

                    <ShowItem when={organizationResource.data}>{org => <>
                        {/* Tabs */}
                        <div class='flex shadow shadow-inset bg-secondary/10'>
                            <For each={processOperator.processes}>{(process, i) =>
                                <div class='cursor-pointer flex b not-first:b-l-solid b-t-solid b-t-4px'
                                    classList={{
                                        'bg-white': processOperator.activeIndex === i(),
                                        'b-t-gray': processOperator.activeIndex !== i(),
                                        'b-t-primary': processOperator.activeIndex === i(),
                                    }}
                                    onclick={() => processOperator.activateByIndex(i())}
                                >
                                    <div class='p-2'
                                        classList={{
                                            'font-bold': processOperator.activeIndex === i()
                                        }}
                                    >
                                        {process.appInfo.name}
                                    </div>
                                    <button type='button' class='pr-2 text-xl'
                                        classList={{
                                            'invisible': processOperator.activeIndex !== i()
                                        }}
                                        onclick={() => processOperator.terminateByIndex(i())}
                                    >
                                        <DismissOutlineIcon />
                                    </button>
                                </div>
                            }</For>
                            <button type='button' class='px-2 text-2xl'
                                onclick={() => { setShowAppDrawer(true) }}
                                title='Launch an app'>
                                <AddOutlineIcon />
                            </button>
                        </div>

                        {/* App */}
                        <div class='overflow-auto grow bg-white/50 shadow'>
                            <For each={processOperator.processes}>{(process, i) =>
                                <div classList={{
                                    'hidden': processOperator.activeIndex !== i()
                                }}>
                                    <Dynamic component={process.appInfo.component} {...{ organization: org, inode: process.inode }} />
                                </div>
                            }</For>
                        </div>
                    </>}</ShowItem>

                </div>
            </div>

            <Show when={showAppDrawer()}>
                <ShowItem when={organizationResource.data}>{org =>
                    <ShowItem when={inodeResource.data}>{inode =>
                        <Dialog
                            title='App Launcher'
                            onDismiss={() => setShowAppDrawer(false)}
                            anchor={DialogAnchor.topCenter}
                            initialPosition={{ x: getScreenCenter().x, y: 100 }}
                        >
                            <AppDrawer org={org} inode={inode} processOperator={processOperator}
                                onLaunched={() => setShowAppDrawer(false)} />
                        </Dialog>
                    }</ShowItem>
                }</ShowItem>
            </Show>

        </MainLayout >
    </SectionedLayout>
}