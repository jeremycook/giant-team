import { createHref } from "./urlHelpers"

const app = (appId: string, organizationId: string, path: string) =>
    inode(organizationId, path) + '?' + new URLSearchParams({ app: appId }).toString();

const inode = (organizationId: string, path: string) =>
    createHref('/o/' + organizationId + (path.length > 0 ? '/' + path : ''));

export const hrefOf = {
    app,
    inode,
    uploadApi: '/api/organization/upload',
}