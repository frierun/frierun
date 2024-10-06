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
                        {item.id}
                        <Uninstall applicationId={item.id} />
                    </li>
                ))}
            </ul>
        </>
    )
}

