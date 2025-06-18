import {useGetApplications} from "@/api/endpoints/applications.ts";

export default function ApplicationTips() {
    const {data, isPending, isError} = useGetApplications();

    if (isPending || isError) return <></>;


    if (data.data.find((item) => item.packageName === "traefik") === undefined) {
        return (
            <>
                <div className={"mt-1"}>
                    → Choose a package from the list below to install it.
                </div>
                <div className={"mt-1 mb-3"}>
                    → We recommend to install the Traefik package first, it will allow to assign subdomains to your apps.
                </div>
            </>
        );
    }

    if (data.data.find((item) => item.packageName === "static-zone") === undefined) {
        return (
            <div className={"mb-3"}>
                → The &quot;static-zone&quot; package must be installed to use Traeffik and route your apps to subdomains.
            </div>
        );
    }


    return <></>;
}

