import { createResource } from "solid-js";
import { postQuery } from "../../../bindings/GiantTeam.Organization.Api.Controllers";
import { TabularData } from "../../../bindings/GiantTeam.Postgres.Models";
import { DataResponseResource } from "../../../helpers/DataResponseResource";

export class QueryResource extends DataResponseResource<TabularData> { }
export function createQueryResource(props: {
    organization: string;
    sql: string;
}) {
    const resourceReturn = createResource(
        () => ({ organizationId: props.organization, sql: props.sql }),
        (props) => postQuery(props)
    );
    return new QueryResource(resourceReturn);
}
