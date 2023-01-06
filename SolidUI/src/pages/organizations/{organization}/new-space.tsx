import { createEffect } from "solid-js";
import { createMutable } from "solid-js/store";
import { postCreateSpace } from "../../../bindings/GiantTeam.Data.Api.Controllers";
import { toast } from "../../../partials/Alerts";
import { PageSettings, go, here } from "../../../partials/Nav";
import { isAuthenticated } from "../../../utils/session";
import { FieldSetOptions, FieldStack } from "../../../widgets/FieldStack";


export const pageSettings: PageSettings = {
    name: 'New Space',
    showInNav: () => isAuthenticated(),
}

export default function NewSpacePage() {
    const data = createMutable({
        name: '',
        schemaName: '',
        schemaNameIsTainted: false,
    });

    const dataOptions: FieldSetOptions = {
        name: { type: 'text', label: 'Space Name', required: true },
        schemaName: {
            type: 'text', label: 'Database Name', required: true, maxLength: 50, pattern: '^[a-z][a-z0-9_]*$',
            title: 'This must start with a lowercase letter, and may be followed by lowercase letters, numbers or the underscore',
            onfocus: () => data.schemaNameIsTainted = true
        },
    };

    createEffect(() => {
        if (!data.schemaNameIsTainted)
            data.schemaName = data.name.toLowerCase().replaceAll(/[^a-z0-9_]+/g, '_').replace(/^[^a-z]+/, '').replace(/[_]+$/, '');
    });

    const formSubmit = async (e: SubmitEvent) => {
        e.preventDefault();

        const response = await postCreateSpace({
            databaseName: here.routeValues.organization!,
            name: data.name,
            schemaName: data.schemaName,
        });

        if (response.ok) {
            toast.info('Space created!');
            go(`/organizations/${here.routeValues.organization}/spaces/` + response.data.spaceId);
        }
        else {
            toast.error(response.message);
        }
    };

    return (
        <section class='card md:w-md md:mx-auto'>

            <h1>New Space</h1>

            <form onSubmit={formSubmit} class='form-grid'>

                <FieldStack data={data} options={dataOptions} />

                <div />
                <div>
                    <button class='button button-primary'>
                        Create Space
                    </button>
                </div>

            </form>

        </section>
    );
}
