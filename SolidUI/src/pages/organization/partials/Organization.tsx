import { For, Show, createSignal } from "solid-js";
import { Dynamic } from "solid-js/web";
import { Inode } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { DismissOutlineIcon, AddOutlineIcon } from "../../../partials/Icons";
import { MainLayout } from "../../../partials/MainLayout";
import { RenderSection } from "../../../partials/Section";
import SectionedLayout from "../../../partials/SectionedLayout";
import Dialog from "../../../widgets/Dialog";
import { ShowItem } from "../../../widgets/ShowItem";
import { OrganizationDetails } from "../OrganizationDetailsResource";
import { AppDrawer } from "./AppDrawer";
import { useInodeExplorerContext } from "./InodeExplorerContext";
import { InodeRoot } from "./InodeTree";
import { useProcessOperatorContext } from "./ProcessOperatorContext";

export function Organization(props: { organization: OrganizationDetails }) {
    const processOperator = useProcessOperatorContext();
    const explorer = useInodeExplorerContext();

    const [showAppDrawer, setShowAppDrawer] = createSignal(false);
    const [activeInode, setActiveInode] = createSignal<Inode>(explorer.root.children?.at(0) ?? explorer.root);

    return <>
        <ShowItem when={props.organization}>{org =>
            <SectionedLayout>
                <MainLayout navBarChildren={<RenderSection name='navbar-start' />}>

                    <div class='flex gap-4 h-100%'>
                        <div class='overflow-auto w-200px'>

                            {/* Navigation */}
                            <InodeRoot
                                inode={explorer.root}
                                selectedInode={activeInode}
                                onClickInode={inode => setActiveInode(inode)} />

                        </div>
                        <div class='grow flex flex-col'>

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
                                        <Dynamic
                                            component={process.appInfo.component}
                                            organization={org}
                                            explorer={explorer}
                                            inode={process.inode} />
                                    </div>
                                }</For>
                            </div>

                        </div>
                    </div>

                    <Show when={showAppDrawer()}>
                        <Dialog
                            title='App Launcher'
                            onDismiss={() => setShowAppDrawer(false)}
                        >
                            <AppDrawer
                                inode={activeInode() ?? explorer.root}
                                processOperator={processOperator}
                                onLaunched={() => setShowAppDrawer(false)}
                            />
                        </Dialog>
                    </Show>

                </MainLayout >
            </SectionedLayout>
        }</ShowItem>
    </>
}