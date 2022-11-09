import { createUniqueId } from "solid-js";

/** Creates a unique but stable ID with a suffix */
export const createId = (suffix: string) => createUniqueId() + "_" + suffix;
