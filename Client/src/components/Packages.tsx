import useAxios from "axios-hooks";
import Install from "./Install.tsx";
import Package from "../models/Package.ts";

export default function Packages() {
    const [{data, loading, error}] = useAxios<Package[]>('/api/v1/packages')

    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error!</p>

    return (
        <>
            <h2 className="text-3xl font-bold underline">
                Packages
            </h2>
            <ul>
                {data.map((item) => (
                    <li key={item.name}>{item.name} <Install packageId={item.name} /></li>
                ))}
            </ul>
        </>
    )
}

