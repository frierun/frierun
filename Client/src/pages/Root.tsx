import Applications from "../components/Applications.tsx";
import Packages from "../components/Packages.tsx";

export default function Root() {
    return (
        <>
            <div id={"header"}>
                <div className={"ml-2 xl:ml-10 flex-shrink-0"}>
                    <img src={'/svg/logo-full.svg'}/>
                </div>
                <div className={"flex-shrink-0 pt-2 xxl:ml-[15%]"}>
                    <img src={'/svg/deco.svg'}/>
                </div>
            </div>
            <div className={"w-full bg-secondary-darker"} style={{height: 4}}>
                <img src={'/svg/deco-stripe.svg'}/>
            </div>
            <Applications />
            <Packages />
        </>
    );
}

