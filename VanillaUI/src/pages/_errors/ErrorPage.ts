import Exception from '../../helpers/Exception';
import { h } from '../../helpers/h';
import CardLayout from "../_ui/CardLayout"

export default function ErrorPage({ error }: { error: unknown }) {
    let errorMessage: string;
    switch (typeof error) {
        case 'string':
            errorMessage = error
            break;

        case 'object':
            if (error instanceof Exception) {
                errorMessage = error.message;
            }
            else {
                errorMessage = JSON.stringify(error, undefined, 4);
            }
            break;

        default:
            errorMessage = 'An unexpected error occurred';
            break;
    }

    return CardLayout(
        h('h1', 'Error'),
        h('pre', errorMessage),
    );
}