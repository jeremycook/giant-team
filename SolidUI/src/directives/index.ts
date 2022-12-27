import { autofocus } from './autofocus';
import { input } from './input';

const directives = {
    autofocus,
    input,
};

Object.entries(directives).forEach(([key, value]) => {
    (globalThis as any)[key] = value;
});
