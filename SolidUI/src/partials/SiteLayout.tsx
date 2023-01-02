import { ParentProps } from "solid-js";
import { RenderSection, SectionContext, SectionContextValue } from "./Section";

export interface SiteLayoutProps extends ParentProps {
}

export default function SiteLayout(props: SiteLayoutProps) {
    const sectionContextValue = new SectionContextValue();

    return (<>
        <SectionContext.Provider value={sectionContextValue}>
            <RenderSection name='siteNav' />
            {props.children}
            <RenderSection name='siteFooter' />
        </SectionContext.Provider>
    </>)
}