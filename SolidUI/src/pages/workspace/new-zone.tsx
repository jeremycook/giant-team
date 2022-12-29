import { useNavigate } from "@solidjs/router";
import { createMutable } from "solid-js/store";
import { CreateNamespace } from "../../api/GiantTeam";
import { postChangeDatabase } from "../../api/GiantTeam.Data.Api";
import { SaveEditFilledIcon } from "../../helpers/icons";
import { Breadcrumb } from "../../utils/nav";
import { FieldStack, FieldStackOptions } from "../../widgets/FieldStack";
import { createWorkspaceUrl, useWorkspaceParams } from "./workspace-layout";

const dataOptions: FieldStackOptions = {
    name: { type: 'text', label: 'Name', required: true },
};

export default function NewZonePage() {
    const navigate = useNavigate();
    const workspaceParams = useWorkspaceParams();

    const data = createMutable({
        name: '',
    });

    async function onsubmitform(e: SubmitEvent) {
        e.preventDefault();

        const output = await postChangeDatabase({
            databaseName: workspaceParams.workspace,
            changes: [
                {
                    $type: 'CreateNamespace',
                    namespaceName: data.name,
                } as CreateNamespace
            ]
        });

        if (output.ok) {
            alert('Zone created!');
            navigate(createWorkspaceUrl('zone', data.name));
            return;
        }
        else {
            alert(output.message);
        }
    }

    return (<>
        <Breadcrumb link={{ text: 'New Zone', href: createWorkspaceUrl('new-zone') }} />

        <section class='pxy md:w-900px max-w-100% md:mx-auto'>
            <h1>New Zone</h1>

            <form onsubmit={onsubmitform}>

                <div class='flex gap-1 mb rounded paint-gray-100'>
                    <button class='button paint-primary'>
                        <SaveEditFilledIcon />
                        Save
                    </button>
                </div>

                <div class='form-grid'>
                    <FieldStack data={data} options={dataOptions} />
                </div>

            </form>
        </section>
    </>);
}