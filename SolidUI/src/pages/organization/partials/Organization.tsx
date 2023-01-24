import { For, Show, createSignal } from "solid-js";
import { Dynamic } from "solid-js/web";
import { Inode, OrganizationDetails } from "../../../bindings/GiantTeam.Organization.Etc.Models";
import { DismissOutlineIcon, AddOutlineIcon } from "../../../partials/Icons";
import { MainLayout } from "../../../partials/MainLayout";
import { RenderSection } from "../../../partials/Section";
import SectionedLayout from "../../../partials/SectionedLayout";
import Dialog from "../../../widgets/Dialog";
import { ShowItem } from "../../../widgets/ShowItem";
import { AppDrawer } from "./AppDrawer";
import { InodeProvider } from "./InodeProvider";
import { InodeRoot } from "./InodeTree";
import { ProcessOperator } from "./ProcessOperatorContext";

export function Organization(props: {
    organization: OrganizationDetails,
    processOperator: ProcessOperator,
    inodeProvider: InodeProvider
}) {
    const [showAppDrawer, setShowAppDrawer] = createSignal(false);
    const [activeInode, setActiveInode] = createSignal<Inode>(props.inodeProvider.root);

    return <>
        <ShowItem when={props.organization}>{org =>
            <SectionedLayout>
                <MainLayout navBarChildren={<RenderSection name='navbar-start' />}>

                    <div class='flex gap-4 h-100%'>
                        <div class='overflow-auto w-200px'>

                            {/* Navigation */}
                            <InodeRoot
                                inodeProvider={props.inodeProvider}
                                selectedInode={activeInode}
                                onClickInode={(e, inode) => {
                                    setActiveInode(inode);
                                    props.processOperator.open(inode);
                                }} />

                        </div>
                        <div class='grow flex flex-col'>

                            {/* Tabs */}
                            <div class='flex shadow shadow-inset bg-secondary/10'>
                                <For each={props.processOperator.processes}>{process =>
                                    <div class='cursor-pointer flex b not-first:b-l-solid b-t-solid b-t-4px'
                                        classList={{
                                            'b-t-gray': props.processOperator.activePid !== process.pid,
                                            'bg-white': props.processOperator.activePid === process.pid,
                                            'b-t-primary': props.processOperator.activePid === process.pid,
                                        }}
                                        onclick={() => props.processOperator.activate(process.pid)}
                                    >
                                        <div class='p-2'
                                            classList={{
                                                'font-bold': props.processOperator.activePid === process.pid
                                            }}
                                        >
                                            {process.appInfo.name}
                                        </div>
                                        <button type='button' class='pr-2 text-xl'
                                            classList={{
                                                'invisible': props.processOperator.activePid !== process.pid
                                            }}
                                            onclick={() => props.processOperator.terminate(process.pid)}
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
                                <For each={props.processOperator.processes}>{process =>
                                    <div classList={{
                                        'hidden': props.processOperator.activePid !== process.pid
                                    }}>
                                        <Dynamic
                                            component={process.appInfo.component}
                                            organization={org}
                                            inodeProvider={props.inodeProvider}
                                            process={process} />
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
                                inode={activeInode() ?? props.inodeProvider.root}
                                processOperator={props.processOperator}
                                onLaunched={() => setShowAppDrawer(false)}
                            />
                        </Dialog>
                    </Show>

                </MainLayout >
            </SectionedLayout>
        }</ShowItem>
    </>
}