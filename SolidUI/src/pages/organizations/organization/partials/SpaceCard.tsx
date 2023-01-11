import { Schema } from "../../../../bindings/GiantTeam.DatabaseDefinition.Models";

interface Space {
    id: string;
    name: string;
    schema: Schema;
}

export default function SpaceCard(props: { data: Space }) {
    return <div class='card'>
        {props.data.name}
    </div>
}
