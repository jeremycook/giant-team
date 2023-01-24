import { ProcessOperator, ProcessOperatorProvider } from "./partials/ProcessOperatorContext";
import { createOrganizationDetailsResource } from "./OrganizationDetailsResource";
import { useParams } from "@solidjs/router";
import { ShowItem } from "../../widgets/ShowItem";
import { InodeProviderContext } from "./partials/InodeProviderContext";
import { InodeProvider } from "./partials/InodeProvider";
import { Organization } from "./partials/Organization";
import { Loading } from "../../partials/Loading";
import { createEffect, createSignal } from "solid-js";
import { OrganizationDetailsProvider } from "./partials/OrganizationDetailsProvider";
import { createInodeListResource } from "./InodeListResource";
import { Inode } from "../../bindings/GiantTeam.Organization.Etc.Models";

export default function OrganizationPage() {
    const params = useParams<{ organization: string }>();

    const organizationResource = createOrganizationDetailsResource({
        organization: params.organization,
    });
    createEffect(() => {
        if (organizationResource.data?.rootInode.name) {
            document.title = organizationResource.data.rootInode.name;
        }
    });

    const inodeListResouce = createInodeListResource({
        organization: params.organization,
        path: '', // All nodes, starting from root
    });
    const [inodeList, setInodeList] = createSignal<Inode[]>([]);
    const [inodeProvider, setInodeProvider] = createSignal<InodeProvider>();
    createEffect(() => {
        const org = organizationResource.data;
        const inodesData = inodeListResouce.data;

        if (org && inodesData) {
            setInodeList(inodesData);
            setInodeProvider(new InodeProvider({
                organization: org,
                inodes: () => inodeList(),
                setInodes: value => setInodeList(value),
            }));
        }
    });

    const processOperator = new ProcessOperator();

    return <>
        <ShowItem when={inodeProvider()} fallback={

            <Loading />

        }>{inodeProvider => <>

            <OrganizationDetailsProvider organizationDetails={inodeProvider.organization}>
                <InodeProviderContext.Provider value={inodeProvider}>
                    <ProcessOperatorProvider processOperator={processOperator}>
                        <Organization organization={inodeProvider.organization} processOperator={processOperator} inodeProvider={inodeProvider} />
                    </ProcessOperatorProvider>
                </InodeProviderContext.Provider>
            </OrganizationDetailsProvider>

        </>}</ShowItem>
    </>
}