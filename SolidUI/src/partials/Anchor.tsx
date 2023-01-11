import { A as SolidRouterA, AnchorProps } from "@solidjs/router";

export function Anchor(props: AnchorProps) {
    return (
        <SolidRouterA {...props}>
            {props.children}
        </SolidRouterA>
    )
}
