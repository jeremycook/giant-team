import { h } from '../../helpers/h'
import CardLayout from "../_ui/CardLayout"

export default function FoundPage({ href }: { href: string }) {
    return CardLayout(
        h('h1', 'Page Found'),
        h('p', 'We believe that the page you are looking for can be found ', h('a', x => x.set({ href }), 'here'), '.'),
    )
}