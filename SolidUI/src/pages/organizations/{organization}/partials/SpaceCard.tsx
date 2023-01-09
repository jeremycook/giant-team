import { Schema } from "../../../../bindings/GiantTeam.DatabaseDefinition.Models";
import { A, here, pageUrl } from "../../../../partials/Nav"

interface Space {
    id: string;
    name: string;
    schema: Schema;
}

export default function SpaceCard(props: { data: Space }) {
    return <div class='card'>
        <A href={pageUrl('/organizations/{organization}/spaces/{space}', { organization: here.routeValues.organization, space: props.data.id })}>{props.data.name}</A>
    </div>
}
