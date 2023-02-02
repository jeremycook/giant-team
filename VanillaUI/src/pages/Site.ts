import { h } from '../helpers/h';
import { SiteNavbarUI as NavbarUI } from "./_ui/Navbar";
import { user } from './login/user';
import HomePage from './HomePage';
import JoinPage from './login/JoinPage';
import LoginPage from './login/LoginPage';
import LogoutPage from './login/LogoutPage';
import MyPage from './my/MyPage';
import OrganizationPage from './organization/OrganizationPage';
import { IRouteDictionary, route, Router } from "./Router";
import { toast, ToastUI } from './_ui/Toast';

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

    router.pagePipe.subscribe(async pipe => {
        // Autofocus after page renders
        await pipe.value;
        setTimeout(() => {
            const els = document.querySelectorAll('[autofocus]');
            if (els.length > 0) {
                (els[0] as unknown as HTMLOrSVGElement).focus();
            }
        });
    });

    return h('.site',
        h('.site-desktop',
            h('.site-navbar', NavbarUI(user)),
            h('.site-router', router.pagePipe),
        ),
        h('.site-toast', ToastUI(toast)),
    )
}