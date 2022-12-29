import { Dynamic } from "solid-js/web";
import { createId } from "../helpers/htmlHelpers";
import BooleanField, { BooleanFieldOptions } from "./BooleanField";
import PasswordField, { PasswordFieldOptions } from "./PasswordFields";
import TextField, { TextFieldOptions } from "./TextField";

export type FieldStackOptions = Record<string, FieldOptions>;

export type FieldOptions =
    BooleanFieldOptions |
    PasswordFieldOptions |
    TextFieldOptions;

const lookup = {
    boolean: BooleanField,
    text: TextField,
    password: PasswordField,
}

export function FieldStack({ data, options: fieldStackOptions }: { data: Record<string, any>, options: FieldStackOptions }) {
    return (<>
        {Object.entries(fieldStackOptions).map(([name, options]) => (<>
            <label for={createId(name)}>{options.label ?? name}</label>
            <Dynamic component={lookup[options.type]} {...{ name, options: options as any, data }} />
        </>))}
    </>)
}
