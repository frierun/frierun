import Applications from "../components/Applications.tsx";
import Packages from "../components/Packages.tsx";

export default function Root() {
    return (
        <>
            <div className={"w-full h-16 bg-primary flex items-center overflow-x-hidden"}>
                <div className={"ml-2 lg:ml-10 flex-shrink-0"}>
                    <img src={'/svg/logo-full.svg'}/>
                </div>
                <div className={"lg:ml-auto lg:mr-20 flex-shrink-0"}>
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

