import { createUniqueId } from "solid-js";

/** Return a string with a random but fixed prefix followed by the provided suffix. */
export const createId = ((prefix) => (suffix: string) => prefix ? prefix + "_" + suffix : suffix)(createUniqueId());
