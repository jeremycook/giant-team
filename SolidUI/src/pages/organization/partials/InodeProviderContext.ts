import { createContext, useContext } from "solid-js";
import { InodeProvider } from "./InodeProvider";


export const InodeProviderContext = createContext<InodeProvider>(undefined! as InodeProvider);

export function useInodeProviderContext() { return useContext(InodeProviderContext); }
