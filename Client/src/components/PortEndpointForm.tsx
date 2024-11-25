import {useEffect, useState} from "react";
import {PortEndpointContract} from "../api/schemas";

type Props = {
    contract: PortEndpointContract;
    updateContract: (contract: PortEndpointContract) => void;
}

export default function PortEndpointForm({contract, updateContract}: Props) {
    const [port, setPort] = useState(contract.destinationPort ?? 0);

    useEffect(() => {
        setPort(contract.destinationPort ?? 0);
    }, [contract]);
    
    const updatePort = (port: string) => {
        let intPort = parseInt(port);
        if (isNaN(intPort)) {
            intPort = 0;
        }
        if (intPort < 0 || intPort > 65535) {
            intPort = 0;
        }
        
        setPort(intPort);
        updateContract({
            ...contract,
            destinationPort: intPort
        });
    }

    return (
        <>
            <div>
                <p>
                    {contract.protocol} endpoint to port {contract.port}
                    {contract.containerName && ` in container ${contract.containerName}`}
                </p>
                <label>
                    Destination port:
                    <input
                        value={port} 
                        onChange={e => updatePort(e.target.value)}
                    />
                </label>
            </div>
        </>
    );
}