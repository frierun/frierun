import {PropsWithChildren} from "react";


type Props = {
    running?: boolean
}
export default function IndicatorStripe({running = false}:PropsWithChildren<Props>)
{
    if(!running)
    return (
        <div className={`w-full from-secondary-darker bg-gradient-to-r via-30% via-secondary to-secondary-darker`} style={{height: 4}}>
        </div>);

    return (
        <div className={`animated-background w-full to-primary bg-gradient-to-r via-secondary from-secondary-darker`} style={{height: 4}}>
        </div>);
}