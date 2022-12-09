import { Icon } from "@iconify-icon/solid";
import chevronDown from '@iconify-icons/ion/chevron-down';
import informationCircle from '@iconify-icons/ion/information-circle';
import person from '@iconify-icons/ion/person';
import warning from '@iconify-icons/ion/warning';

export const ChevronDown = (props: any) => <Icon icon={chevronDown} aria-hidden {...props} />;
export const InfoIcon = (props: any) => <Icon icon={informationCircle} aria-hidden {...props} />;
export const Profile = (props: any) => <Icon icon={person} aria-hidden {...props} />;
export const WarningIcon = (props: any) => <Icon icon={warning} aria-hidden {...props} />;
