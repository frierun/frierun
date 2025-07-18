﻿import {useEffect, useState} from "react";
import {PortEndpoint} from "@/api/schemas";

type Props = {
    contract: PortEndpoint;
    updateContract: (contract: PortEndpoint) => void;
}

export default function PortEndpointForm({contract, updateContract}: Props) {
    const [port, setPort] = useState(contract.externalPort);

    useEffect(() => {
        setPort(contract.externalPort);
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
            externalPort: intPort
        });
    }

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>
                        {contract.protocol} endpoint to port 
                    </label>
                    {contract.port}
                    {contract.container && ` in container ${contract.container}`}
                </div>
                <label className={"inline-block w-48"}>
                    Destination port:
                </label>
                <input
                    value={port}
                    onChange={e => { updatePort(e.target.value); }}
                />
            </div>
        </>
    );
}