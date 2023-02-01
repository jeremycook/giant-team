import { h, BaseNode } from '../../helpers/h';

export default function CardLayout(...children: BaseNode[]) {
    return h('main.site-card',
        h('div.card',
            ...children
        )
    );
}
