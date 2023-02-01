import { h } from '../../helpers/h';
import BooleanField, { BooleanFieldOptions } from "./BooleanField";
import PasswordField, { PasswordFieldOptions } from "./PasswordFields";
import TextField, { TextFieldOptions } from "./TextField";

export type FieldSetOptions = Record<string, FieldOptions>;

export type FieldOptions =
    BooleanFieldOptions |
    PasswordFieldOptions |
    TextFieldOptions;

const lookup = {
    boolean: BooleanField,
    text: TextField,
    password: PasswordField,
}

export default function FieldStack({ data, options: fieldSetOptions }: { data: Record<string, any>, options: FieldSetOptions }) {
    const fields = Object.entries(fieldSetOptions).map(([name, options]) => ({
        name,
        options,
        Component: lookup[options.type] as any,
    }));

    return fields.flatMap(({ name, options, Component }) => ([
        h('label', options.label ?? name),
        h('div',
            Component({ name: name, options: options, data: data }),
        ),
    ]));
}
