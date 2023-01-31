import { html } from '../../helpers/html'
import CardLayout from "../_ui/CardLayout"

export default function NotFoundPage(props: { href: string }) {
    return CardLayout(
        undefined,
        html('h1', h => h.append('Page Not Found')),
        html('p', h => h.append(`The page or resource you were looking for was not found at ${props.href}.`)),
    );
}