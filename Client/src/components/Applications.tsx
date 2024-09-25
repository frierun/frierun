import useAxios from "axios-hooks";
import Application from "../models/Application.ts";
import Uninstall from "./Uninstall.tsx";

export default function Applications() {
    const [{data, loading, error}] = useAxios<Application[]>('/api/v1/applications')

    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error!</p>

    return (
        <>
            <h2 className="text-3xl font-bold underline">
                Applications
            </h2>
            <ul>
                {data.map((item) => (
                    <li key={item.id}>
                        {item.id}
                        <Uninstall applicationId={item.id}/>
                    </li>
                ))}
            </ul>
        </>
    )
}

