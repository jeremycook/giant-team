import Router from "./Router";
import { Navbar } from "./_ui/Navbar";

export default function Site() {
    return <div class='site'>
        <Navbar />
        {<Router />}
    </div>
}