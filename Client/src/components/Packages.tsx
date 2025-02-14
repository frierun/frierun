import {Link} from "react-router-dom";
import Button from "./Button";
import {useGetPackages} from "@/api/endpoints/packages";

export default function Packages() {
    const {data, isPending, isError} = useGetPackages();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    const colors = {
        default: '#ccc',
        internet: '#a7aadd',
        network: '#89c1ff',
        storage: '#b5e3ca',
        dev: '#b3c422',
        dashboard: '#e3b5d4',
    }
    return (
        <div className={"my-6"}>
            <h1 className="">
                Packages
            </h1>
            <div className={"grid lg:grid-cols-3 xxl:grid-cols-4 gap-3"}>
                {data.data.map((item) => (
                    <div key={item.name} className={"bg-gray p-2 lg:p-3 rounded-md flex justify-between items-center"}>
                        <div className={"flex gap-3"}>
                            <div className={"h-12 w-12 rounded flex-shrink-0"}>
                                <img src={`/packages/${item.name}.png`} className={"rounded"}/>
                            </div>
                            <div>
                                <div className={"text-bold text-md font-bold"}>{item.name}</div>
                                <div className={"my-2"}>
                                    {item.shortDescription ?? ''}
                                </div>
                                <div className={"mt-1 flex gap-1"}>
                                    {item.tags.map(tag => (
                                        <div
                                            key={tag}
                                            className={`rounded-full px-3 py-0.5 text-sm font-bold`}
                                            style={{backgroundColor: colors[tag] ?? '#ddd'}}
                                        >
                                            {tag}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>
                        <div>
                            <Link to={`/packages/${item.name}`}>
                                <Button type={"primary"}>
                                    Install
                                </Button>
                            </Link>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}

