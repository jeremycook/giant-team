import { createHref } from "./urlHelpers"

export const link = {
    datum: (organizationId: string, path: string) => {
        return createHref('/o/' + organizationId + '/' + path);
    }
}