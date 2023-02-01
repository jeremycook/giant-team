import { h } from '../../helpers/h';
import CardLayout from "../_ui/CardLayout"

export default function NotFoundPage(props: { href: string }) {
    return CardLayout(
        h('h1', 'Page Not Found'),
        h('p', `We did not find a page or resource at ${props.href}.`),
    );
}