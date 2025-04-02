import {useGetApplications} from "@/api/endpoints/applications.ts";

export default function ApplicationTips() {
    const {data, isPending, isError} = useGetApplications();

    if (isPending || isError) return <></>;

    if (data.data.find((item) => item.packageName === "traefik") === undefined) {
        return (
            <div className={"mb-3"}>
                You can install Traefik to route http traffic to different applications
            </div>
        );
    }

    if (data.data.find((item) => item.packageName === "static-domain") === undefined) {
        return (
            <div className={"mb-3"}>
                You should install the &quot;static-domain&quot; package to specify domain, which traefik can use. 
                Use &quot;localhost&quot; for local environment or domain you brought and pointed to the server.
            </div>
        );
    }
    
    
    return <></>;
}

