import { A } from "@solidjs/router";
import { For } from "solid-js";
import { hrefOf } from "../../../helpers/links";
import { AppsIcon, AppsOutlineIcon } from "../../../partials/Icons";
import { Section } from "../../../partials/Section";
import { ShowItem } from "../../../widgets/ShowItem";
import { ProcessOperator } from "../ProcessOperator";
import { InodeResource } from "../InodeResource";
import { OrganizationDetailsResource } from "../OrganizationDetailsResource";

export function Explorer(props: { processOperator: ProcessOperator, organization: OrganizationDetailsResource, inode: InodeResource }) {

    const segments = () => props.inode.data && props.inode.data.path.length > 0 ?
        props.inode.data.path.split('/').reduce((prev, name) => ([{ name, path: prev[prev.length - 1]?.path + '/' + name }, ...prev]), [] as { name: string, path: string }[]) :
        [];

    return <>
        <ShowItem when={props.organization.data}>{org => <>
            <ShowItem when={props.inode.data}>{inode => <>

                <Section name='navbar-start'>

                    <div class='mr-auto flex children:p-2 children:text-light'>
                        <div class='dropdown'>
                            <button class='dropdown-button text-light' type='button' id='navbar-start-apps-dropdown'>
                                <AppsIcon class='parent-active' />
                                <AppsOutlineIcon class='parent-inactive' />
                                <span class='sr-only'> Apps</span>
                            </button>
                            <div class='dropdown-anchor' aria-labelledby='navbar-start-apps-dropdown'>
                                <div class='dropdown-content stack bg-dark rounded shadow children:text-light'>
                                    <For each={org.apps}>{app => <>
                                        <button type='button' class='stack-item' onclick={() => { props.processOperator.launch(app, inode); }}>
                                            {app.name}
                                        </button>
                                    </>}</For>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class='mx-auto flex'>
                        <div class='flex children:p-2 children:text-light'>
                            <div class='dropdown'>
                                <button class='dropdown-button text-light' type='button' id='navbar-start-breadcrumb-dropdown'>
                                    {inode.name}
                                </button>
                                <div class='dropdown-anchor' aria-labelledby='navbar-start-breadcrumb-dropdown'>
                                    <div class='dropdown-content stack bg-dark rounded shadow children:text-light'>
                                        <For each={segments()}>{segment => <>
                                            <A class='stack-item' href={hrefOf.inode(org.organizationId, segment.path)}>{segment.name}</A>
                                        </>}</For>
                                        <A class='stack-item' href={hrefOf.inode(org.organizationId, '')}>{org.name}</A>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </Section>

                <div class='flex flex-wrap gap-4'>
                    <For each={inode.children}>{child => (<>
                        <div class='card'>
                            <A href={hrefOf.inode(org.organizationId, child.path)}>{child.name}</A>
                        </div>
                    </>)}</For>
                </div>

            </>}</ShowItem>
        </>}</ShowItem>
    </>
}