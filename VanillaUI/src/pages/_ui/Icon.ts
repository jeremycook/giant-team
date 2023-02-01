import { svg } from '../../helpers/h';

const iconLookup = {
    'home-12-regular': () => import('@iconify-icons/fluent/home-12-regular'),
    'person-12-regular': () => import('@iconify-icons/fluent/person-12-regular'),
    'sparkle-16-regular': () => import('@iconify-icons/fluent/sparkle-16-regular'),
    'alert-12-regular': () => import('@iconify-icons/fluent/alert-12-regular'),
    'caret-right-12-regular': () => import('@iconify-icons/fluent/caret-right-12-regular'),
}

type IconType = keyof typeof iconLookup;

// Iconify doesn't provide dimensions when for *-16-* icons
// so that must be the default.
const fallbackDim = 16;
const initialViewbox = '0 0 16 16';

export default function Icon({ icon, ...props }: { icon: IconType } & { [k: string]: string | number | boolean }) {
    props.class = (props.class ?? '') + ' icon';
    const element = svg('svg');
    element.setAttributes({ viewBox: initialViewbox, 'aria-hidden': true, ...props });

    // The contents will be filled as soon as it is available.
    iconLookup[icon]().then(module => {
        const data = module.default;
        element.setAttribute('viewBox', `0 0 ${data.width ?? fallbackDim} ${data.height ?? fallbackDim}`);
        element.innerHTML = data.body;
    });

    return element;
}

declare global {
    interface Element {
        setAttributes(attributes: { [k: string]: string | number | boolean }): void;
    }
}

Element.prototype.setAttributes = function (this: Element, attributes: { [k: string]: string | number | boolean }): void {
    for (const key in attributes) {
        const value = attributes[key];
        switch (typeof value) {
            case 'string':
                this.setAttribute(key, value);
                break;
            case 'number':
                this.setAttribute(key, value.toString());
                break;
            case 'boolean':
                if (value)
                    this.setAttribute(key, '');
                else
                    this.removeAttribute(key);
                break;
            default:
                throw { message: 'Unsupported attribute {value}.', value, typeof: typeof value };
        }
    }
}
