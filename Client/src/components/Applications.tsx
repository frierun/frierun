import Uninstall from "./Uninstall.tsx";
import {useGetApplications} from "../api/endpoints/applications.ts";


export default function Applications() {
    const {data, isPending, isError} = useGetApplications();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <div className={"mx-2 xl:mx-10 my-6"}>
            <h2 className="text-xl font-bold mb-4">
                Installed Applications
            </h2>
            {data.data.length === 0 && <div className={"mb-3"}>Select a package you like and install it. You can install severall instances of the same package.</div>}
            <div className={"grid grid-cols-1 lg:grid-cols-2 xxl:grid-cols-3 gap-4"}>
                {data.data.map((item) => (
                    <div className={"bg-gray p-2 lg:p-4 rounded-md flex justify-between"} key={item.id}>
                        <div className={"flex gap-3"}>
                        <div className={"h-24 w-24 rounded flex-shrink-0"}>
                            <img src={`/packages/${item.name}.png`} className={"rounded"}/>
                        </div>
                        <div>
                            <div className={"font-bold text-lg"}>
                                {item.name}
                            </div>
                            <div className={"text-sm"}>
                                {item.id}
                            </div>
                            {item.serviceUrl && (
                                <div>
                                    <a href={item.serviceUrl} target="_blank" rel="noreferrer noopener" className={"text-black"}>
                                        <span className="text-blue-500">{item.serviceUrl}</span>
                                    </a>
                                </div>
                            )}
                        </div>
                        </div>
                        <div>
                            <Uninstall applicationId={item.id}/>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}

