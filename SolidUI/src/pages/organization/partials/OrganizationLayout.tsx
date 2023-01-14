import { Outlet } from "@solidjs/router";
import { Suspense } from "solid-js";
import { Loading } from "../../../partials/Loading";
import { MainLayout } from "../../../partials/MainLayout";
import { RenderSection } from "../../../partials/Section";
import SectionedLayout from "../../../partials/SectionedLayout";

export default function OrganizationLayout() {
    return <SectionedLayout>
        <MainLayout navBarChildren={<RenderSection name='navbar-start' />}>
            <Suspense fallback={<Loading />}>
                <Outlet />
            </Suspense>
        </MainLayout >
    </SectionedLayout>
}