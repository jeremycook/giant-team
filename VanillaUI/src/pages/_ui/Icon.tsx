import { svg } from '../../helpers/h';
import { SvgSVGAttributes } from '../../helpers/jsx/jsx';

const iconLookup = {
    'home-12-filled': () => import('@iconify-icons/fluent/home-12-filled'),
    'home-12-regular': () => import('@iconify-icons/fluent/home-12-regular'),
}

type IconType = keyof typeof iconLookup;

export default function ({ icon, ...props }: { icon: IconType } & SvgSVGAttributes<SVGSVGElement>) {
    const element = svg('svg', { viewBox: '0 0 12 12', class: 'icon', ...props });

    // The contents will be filled as soon as it is available.
    iconLookup[icon]().then(module => {
        const data = module.default;
        element.setAttribute('viewBox', `0 0 ${data.width} ${data.height}`);
        element.innerHTML = data.body;
    });

    return element;
}
