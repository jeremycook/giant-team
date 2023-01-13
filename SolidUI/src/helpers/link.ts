import { createHref } from "./urlHelpers"

export const link = {
    inode: (organizationId: string, path: string) => {
        return createHref('/o/' + organizationId + '/' + path);
    }
}