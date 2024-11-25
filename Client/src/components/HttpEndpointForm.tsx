import {useEffect, useState} from "react";
import {HttpEndpointContract} from "../api/schemas";

type Props = {
    contract: HttpEndpointContract;
    updateContract: (contract: HttpEndpointContract) => void;
}

export default function HttpEndpointForm({contract, updateContract}: Props) {
    const [domain, setDomain] = useState(contract.domainName ?? '');

    useEffect(() => {
        setDomain(contract.domainName ?? '');
    }, [contract]);
    
    if (contract.providerType !== 'TraefikHttpEndpointProvider') {
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
                <p>
                    Http endpoint to port {contract.port}
                    {contract.containerName && ` in container ${contract.containerName}`}
                </p>
                <label>
                    Domain:
                    <input value={domain} onChange={e => updateDomain(e.target.value)}/>
                </label>
            </div>
        </>
    );
}