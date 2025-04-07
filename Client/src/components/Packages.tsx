import {useGetPackages} from "@/api/endpoints/packages";
import Package from "@/components/Package.tsx";

export default function Packages() {
    const {data, isPending, isError} = useGetPackages();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <>
            <div className={"my-12"}>
                <h1>Provider packages</h1>
                <div className={"mb-4"}>These packages may be needed to install other packages</div>
                <div>
                    <div className={"grid lg:grid-cols-3 xxl:grid-cols-4 gap-3"}>
                        {data.data
                            .filter(item => item.tags.includes('provider'))
                            .map((item) => (
                                <Package key={item.name} pkg={item}/>
                            ))}
                    </div>
                </div>
            </div>
            <div className={"my-12"}>
                <h1>Packages
                </h1>
                <div className={"grid lg:grid-cols-3 xxl:grid-cols-4 gap-3"}>
                    {data.data
                        .filter(item => !item.tags.includes('provider'))
                        .map((item) => (
                            <Package key={item.name} pkg={item}/>
                        ))}
                </div>
            </div>
        </>
    )
}

