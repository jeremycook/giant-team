import { createHref } from "./urlHelpers"

export const hrefOf = {
    inode: (organizationId: string, path: string) => {
        return createHref('/o/' + organizationId + '/' + path);
    }
}