import { h, HAttributes } from '../../helpers/h';

export interface BooleanFieldOptions extends HAttributes {
    type: 'boolean';
    label: string;
}

export default function BooleanField({ name, options, data }: { name: string; options: BooleanFieldOptions; data: Record<string, any>; }) {
    const {
        type,
        label,
        ...attributes
    } = options;

    return h('div',
        h('label',
            h('input', x => x.set({
                type: 'radio',
                name: name,
                checked: data[name] === true,
                onchange: _ => data[name] = true,
                required: true,
                ...attributes,
            })), ' Yes',
            h('input', x => x.set({
                type: 'radio',
                name: name,
                checked: data[name] !== true,
                onchange: _ => data[name] = false,
                required: true,
                ...attributes,
            })), ' No',
        )
    )
}
