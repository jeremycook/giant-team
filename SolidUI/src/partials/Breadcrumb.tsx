import { For, Show } from "solid-js";
import { reveal } from "../helpers/objectHelpers";
import { pageRoutes, pageUrl, here, A } from "./Nav";

export default function Breadcrumb() {
    return (
        <ul>
            <For each={pageRoutes}>{page => {
                const href = pageUrl(page.route, here.routeValues);
                return <Show when={href && here.pathname.startsWith(href)}>
                    <li>
                        <A href={href}>{reveal(page.name)}</A>
                    </li>
                </Show>
            }
            }</For>
        </ul>
    )
}