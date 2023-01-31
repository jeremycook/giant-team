import { Elem, ElemAttributes, html } from '../../helpers/html';

export default function CardLayout(attributes?: ElemAttributes, ...children: Elem[]) {
    return html('main', h => h.set(attributes)
        .append(
            html('div', h => h.set({ class: 'card' })
                .append(...children))
        ));
}
