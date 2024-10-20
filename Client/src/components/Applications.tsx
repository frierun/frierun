import Uninstall from "./Uninstall.tsx";
import {useGetApplications} from "../api/endpoints/applications.ts";

export default function Applications() {
    const {data, isPending, isError} = useGetApplications();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <>
            <h2 className="text-3xl font-bold underline">
                Applications
            </h2>
            <ul>
                {data.data.map((item) => (
                    <li key={item.id}>
                        <div>
                            {item.name} ({item.id})
                        </div>
                        {item.serviceUrl && (
                            <div>
                                <a href={item.serviceUrl} target="_blank" rel="noreferrer noopener">
                                    <span className="text-blue-500">{item.serviceUrl}</span>
                                </a>
                            </div>
                        )}
                        <div>
                            <Uninstall applicationId={item.id}/>
                        </div>
                    </li>
                ))}
            </ul>
        </>
    )
}

