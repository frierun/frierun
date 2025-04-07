import Uninstall from "./Uninstall.tsx";
import {useGetApplications} from "@/api/endpoints/applications.ts";
import ApplicationTips from "@/components/ApplicationTips.tsx";

export default function Applications() {
    const {data, isPending, isError} = useGetApplications();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <div className={"my-6"}>
            <h1 className="">
                Installed Applications
            </h1>
            <ApplicationTips />
            <div className={"grid grid-cols-1 lg:grid-cols-2 xxl:grid-cols-3 gap-4"}>
                {data.data.map((item) => (
                    <div className={"bg-gray p-2 lg:p-4 rounded-md flex justify-between"} key={item.name}>
                        <div className={"flex gap-3"}>
                        <div className={"h-24 w-24 rounded flex-shrink-0"}>
                            {item.packageName && (
                                <img 
                                    alt={item.packageName}
                                    src={`/packages/${item.packageName}.png`} 
                                    className={"rounded"}
                                />
                            )}
                        </div>
                            <div>
                                <div className={"font-bold text-lg"}>
                                    {item.name}
                                </div>
                                {item.url && (
                                    <div>
                                        <a href={item.url} target="_blank" rel="noreferrer noopener"
                                           className={"text-black"}>
                                            <span className="font-bold text-primary text-sm">{item.url}</span>
                                        </a>
                                    </div>
                                )}
                                {item.description && (
                                    <div className={"text-sm"}>
                                        {item.description}
                                    </div>
                                )}
                            </div>
                        </div>
                        <div>
                            <Uninstall name={item.name}/>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}

