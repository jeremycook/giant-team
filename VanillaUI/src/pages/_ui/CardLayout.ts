import { h, BaseNode } from '../../helpers/h';

export default function CardLayout(...children: BaseNode[]) {
    return h('main',
        h('div', c => c.set({ class: 'card' }),
            ...children
        )
    );
}
