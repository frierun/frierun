import {ReactElement} from "react";


type Props = {
    disabled?: boolean
    onClick?: () => void
    type?: 'default' | 'primary';
    children: Array<ReactElement>
}
export default function Button({disabled = false, onClick = ()=>{}, type = 'default', children}:Props)
{
    if(type === 'default')
        return (
            <button
                disabled={disabled}
                className="rounded border-2 border-primary text-primary px-5
                py-1 font-bold hover:border-secondary hover:text-primary-softer hover:bg-transparent ring-0"
                onClick={onClick}
            >
                {children}
            </button>
        );

    return (
        <button
            disabled={disabled}
            className="btn rounded font-bold border-2 border-primary bg-primary text-secondary-lighter hover:text-secondary px-5 py-1 text-secondary"
            onClick={onClick}
        >
            {children}
        </button>);
}