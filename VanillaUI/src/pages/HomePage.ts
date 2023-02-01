import { h } from '../helpers/h';
import CardLayout from './_ui/CardLayout'

export default function () {
    return CardLayout(
        h('h1', 'Welcome'),
        h('p', 'Content coming soon!'),
    );
}