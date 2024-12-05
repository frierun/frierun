import {useEffect, useState} from "react";
import {HttpEndpoint} from "@/api/schemas";

type Props = {
    contract: HttpEndpoint;
    updateContract: (contract: HttpEndpoint) => void;
}

export default function HttpEndpointForm({contract, updateContract}: Props) {
    const [domain, setDomain] = useState(contract.domainName ?? '');

    useEffect(() => {
        setDomain(contract.domainName ?? '');
    }, [contract]);
    
    if (contract.installerType !== 'TraefikHttpEndpointProvider') {
        return <></>;
    }
    
    const updateDomain = (domainName: string) => {
        setDomain(domainName);
        updateContract({
            ...contract,
            domainName
        });
    }

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>Http endpoint to port </label>{contract.port}
                    {contract.containerName && ` in container ${contract.containerName}`}
                </div>
                <label className={"inline-block w-48"}>
                    Domain:
                </label>
                <input value={domain} onChange={e => updateDomain(e.target.value)}/>
            </div>
        </>
    );
}