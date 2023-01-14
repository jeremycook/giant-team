import { JSX, ParentProps } from 'solid-js';
import { Navbar } from './Navbar';
import { Body } from './Body';

export function MainLayout(props: { navBarChildren?: JSX.Element } & ParentProps) {
    return (
        <Body class='bg-gradient-from-white bg-gradient-to-primary bg-gradient-radial bg-no-repeat bg-contain p-4px'>
            <div class='flex flex-col h-[calc(100vh-(2*4px))] shadow rounded-2'>
                <Navbar>{props.navBarChildren}</Navbar>
                <main role='main' class='grow overflow-auto p-2 bg-white/95 rounded-b-2'>
                    {props.children}
                </main>
            </div>
        </Body>
    );
};
