import { Icon } from "@iconify-icon/solid";
import chevronDown from '@iconify-icons/ion/chevron-down';
import home from '@iconify-icons/ion/home';
import informationCircle from '@iconify-icons/ion/information-circle';
import person from '@iconify-icons/ion/person';
import personOutline from '@iconify-icons/ion/person-outline';
import search from '@iconify-icons/ion/search';
import warning from '@iconify-icons/ion/warning';

export const ChevronDownIcon = (props: any) => <Icon icon={chevronDown} aria-hidden {...props} />;
export const HomeIcon = (props: any) => <Icon icon={home} aria-hidden {...props} />;
export const SearchIcon = (props: any) => <Icon icon={search} aria-hidden {...props} />;
export const InfoIcon = (props: any) => <Icon icon={informationCircle} aria-hidden {...props} />;
export const ProfileIcon = (props: any) => <Icon icon={person} aria-hidden {...props} />;
export const PersonOutlineIcon = (props: any) => <Icon icon={personOutline} aria-hidden {...props} />;
export const WarningIcon = (props: any) => <Icon icon={warning} aria-hidden {...props} />;
