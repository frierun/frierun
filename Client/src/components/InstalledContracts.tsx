import {useGetContracts} from "@/api/endpoints/contracts.ts";
import InstalledContract from "@/components/InstalledContract.tsx";
import DiscoverButton from "@/components/DiscoverButton.tsx";

export default function InstalledContracts() {
    const {data, isPending, isError} = useGetContracts();

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <>
            <div className={"my-12"}>
                <h1>Installed contracts</h1>
                <div className={"mb-4"}>
                    <DiscoverButton />
                </div>
                <div className={"grid lg:grid-cols-3 xxl:grid-cols-4 gap-3"}>
                    {data.data
                        .map((item) => (
                            <InstalledContract key={item.id} contract={item}/>
                        ))}
                </div>
            </div>

        </>
    );
}