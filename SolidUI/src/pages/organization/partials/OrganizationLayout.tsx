import { Outlet } from "@solidjs/router";
import { Suspense } from "solid-js";
import { Loading } from "../../../partials/Loading";
import { MainLayout } from "../../../partials/MainLayout";

export default function OrganizationLayout() {
    return <MainLayout>
        <Suspense fallback={<Loading />}>
            <Outlet />
        </Suspense>
    </MainLayout >
}