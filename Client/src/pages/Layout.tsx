import {PropsWithChildren, useContext} from "react";
import IndicatorStripe from "../components/IndicatorStripe";
import StateContext from "@/providers/StateContext.tsx";

type Props = {}
export default function Layout({children}: PropsWithChildren<Props>) {
    const {ready} = useContext(StateContext);

    return <>
        {!ready &&
            <div
                className={"z-50 bg-gray opacity-50 absolute top-20 left-0 right-0 bottom-0 flex justify-center font-bold"}>
                Please wait...
            </div>
        }
        <div id={"header"}>
            <div className={"frame flex-shrink-0"}>
                <img src={'/svg/logo-full.svg'}/>
            </div>
            <div className={"flex-shrink-0 pt-2 xxl:ml-[15%]"}>
                <img src={'/svg/deco.svg'}/>
            </div>
        </div>
        <IndicatorStripe running={!ready}/>
        <div className={"frame my-6"}>
            {children}
        </div>
    </>
}