import HomePage from './HomePage';
import JoinPage from './login/JoinPage';
import LoginPage from './login/LoginPage';
import LogoutPage from './login/LogoutPage';
import MyPage from './my/MyPage';
import OrganizationPage from './organization/OrganizationPage';
import Router, { IRoutes } from "./Router";
import { Navbar } from "./_ui/Navbar";

export default function Site() {

    const routes: IRoutes = {
        '/': HomePage,
        '/join': JoinPage,
        '/login': LoginPage,
        '/logout': LogoutPage,
        '/my': MyPage,
        '/o/(?<organization>[0-9a-f-]{32,36})': OrganizationPage,
    }

    return <div class='site'>
        <Navbar />
        <Router routes={routes} />
    </div>
}