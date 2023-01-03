import { Schema } from "../../../../api/GiantTeam";
import { A, here, pageUrl } from "../../../../partials/Nav"

interface Space {
    id: string;
    name: string;
    schema: Schema;
}

export default function SpaceCard(props: { data: Space }) {
    return <div class='card'>
        <A href={pageUrl('/organizations/{organization}/spaces/{space}', { ...here.routeValues, space: props.data.id })}>{props.data.name}</A>
    </div>
}
