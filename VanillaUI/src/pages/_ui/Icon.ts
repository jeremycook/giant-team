import { svg } from '../../helpers/h';
import { Pipe } from '../../helpers/Pipe';

const iconLookup = {
    'alert-12-regular': () => import('@iconify-icons/fluent/alert-12-regular'),
    'caret-right-12-regular': () => import('@iconify-icons/fluent/caret-right-12-regular'),
    'home-12-regular': () => import('@iconify-icons/fluent/home-12-regular'),
    'mail-read-16-regular': () => import('@iconify-icons/fluent/mail-read-16-regular'),
    'mail-unread-16-regular': () => import('@iconify-icons/fluent/mail-unread-16-regular'),
    'person-12-regular': () => import('@iconify-icons/fluent/person-12-regular'),
    'sparkle-16-regular': () => import('@iconify-icons/fluent/sparkle-16-regular'),
}

export type IconType = keyof typeof iconLookup;

// Iconify doesn't provide dimensions when for *-16-* icons
// so that must be the default.
const fallbackDim = 16;
const initialViewbox = '0 0 16 16';

export default function Icon(type: IconType | Pipe<IconType>) {
    const element = svg('svg');
    element.setAttribute('viewBox', initialViewbox)
    element.setAttribute('aria-hidden', '');
    element.classList.add('icon');

    if (typeof type === 'string') {
        // The contents will be filled as soon as it is available.
        iconLookup[type]().then(module => {
            const data = module.default;
            element.setAttribute('viewBox', `0 0 ${data.width ?? fallbackDim} ${data.height ?? fallbackDim}`);
            element.innerHTML = data.body;
        });
    }
    else {
        const listener = (signal: Pipe<IconType>): void => {
            iconLookup[signal.value]().then(module => {
                const data = module.default;
                element.setAttribute('viewBox', `0 0 ${data.width ?? fallbackDim} ${data.height ?? fallbackDim}`);
                element.innerHTML = data.body;
            });
        };
        listener(type);

        const unsubscribeToken = type.subscribe(listener);
        element.addEventListener('dispose', _ => type.unsubscribe(unsubscribeToken));
    }

    return element;
}
