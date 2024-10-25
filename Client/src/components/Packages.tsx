import {Link} from "react-router-dom";
import {useGetPackages} from "../api/endpoints/packages.ts";
import Button from "./Button";

export default function Packages() {
    const {data, isPending, isError} = useGetPackages();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <div className={"mx-2 xl:mx-10 my-6"}>
            <h2 className="text-xl font-bold mb-4">
                Packages
            </h2>
            <div className={"grid lg:grid-cols-3 xxl:grid-cols-4 gap-3"}>
                {data.data.map((item) => (
                    <div key={item.name} className={"bg-gray p-2 lg:p-3 rounded-md flex justify-between items-center"}>
                        <div className={"flex gap-3 items-center"}>
                            <div className={"h-12 w-12 rounded flex-shrink-0"}>
                                <img src={`/packages/${item.name}.png`} className={"rounded"}/>
                            </div>
                        <div className={"text-bold text-md font-bold"}>{item.name}</div>
                        </div>
                        <Button type={"primary"}>
                            <Link to={`/packages/${item.name}`}>Install</Link>
                        </Button>
                    </div>
                ))}
            </div>
        </div>
    )
}

