import {Link} from "react-router-dom";
import {useGetPackages} from "../api/endpoints/packages.ts";

export default function Packages() {
    const {data, isPending, isError} = useGetPackages();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <>
            <h2 className="text-3xl font-bold underline">
                Packages
            </h2>
            <ul>
                {data.data.map((item) => (
                    <li key={item.name}>
                        {item.name}
                        <Link to={`/packages/${item.name}`}>Install</Link>
                    </li>
                ))}
            </ul>
        </>
    )
}

