import {PropsWithChildren} from "react";

type Props = {
    disabled?: boolean
    onClick?: () => void
    type?: 'default' | 'primary';
}
export default function Button(
    {
        disabled = false,
        onClick = () => {
        },
        type = 'default',
        children
    }: PropsWithChildren<Props>
) {
    if (type === 'default')
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
            type={"button"}
            className="btn btn-primary rounded font-bold border-2 border-primary bg-primary
            text-secondary-lighter hover:text-secondary-softer px-5 py-1"
            onClick={onClick}
        >
            {children}
        </button>);
}