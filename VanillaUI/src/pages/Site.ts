import { h } from '../helpers/h';
import { Navbar } from "./_ui/Navbar";
import { user } from './login/user';
import HomePage from './HomePage';
import JoinPage from './login/JoinPage';
import LoginPage from './login/LoginPage';
import LogoutPage from './login/LogoutPage';
import MyPage from './my/MyPage';
import OrganizationPage from './organization/OrganizationPage';
import RouterUI, { IRouteDictionary, route, Router } from "./Router";

export default function Site() {

    const routes: IRouteDictionary = {
        '/': HomePage,
        '/join': JoinPage,
        '/login': LoginPage,
        '/logout': LogoutPage,
        '/my': MyPage,
        '/o/(?<organization>[0-9a-f-]{32,36})': OrganizationPage,
    }

    const router = new Router(route, routes);

    router.pipe.subscribe(async pipe => {
        // Try to autofocus after rendering finishes
        await pipe.value;
        setTimeout(() => {
            const els = document.querySelectorAll('[autofocus]');
            if (els.length > 0) {
                (els[0] as unknown as HTMLOrSVGElement).focus();
            }
        }, 0);
    });

    return h('.site',
        Navbar(user),
        RouterUI(router),
    );
}