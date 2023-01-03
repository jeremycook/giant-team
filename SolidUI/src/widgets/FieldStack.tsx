import { Dynamic } from "solid-js/web";
import { createId } from "../helpers/htmlHelpers";
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

export function FieldStack({ data, options: fieldSetOptions }: { data: Record<string, any>, options: FieldSetOptions }) {
    return (<>
        {Object.entries(fieldSetOptions).map(([name, options]) => (<>
            <label for={createId(name)}>{options.label ?? name}</label>
            <div>
                <Dynamic component={lookup[options.type]} {...{ name, options: options as any, data }} />
            </div>
        </>))}
    </>)
}
