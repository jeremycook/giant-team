import { BaseNode, h } from '../../helpers/h'

export default function MainLayout(...children: BaseNode[]) {
    return h('main', m => m.set({ role: 'main', class: 'site-main' }),
        ...children
    )
}
