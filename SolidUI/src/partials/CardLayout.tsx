import { ParentProps } from 'solid-js';
import { MainLayout } from './MainLayout';

export function CardLayout(props: ParentProps) {
    return (
        <MainLayout>
            <div class='card md:w-md md:mx-auto mxy'>
                {props.children}
            </div>
        </MainLayout>
    );
};
