import {useEffect, useState} from "react";
import {PortEndpoint} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function PortEndpointForm({contract, updateContract}: ContractProps<PortEndpoint>) {
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
        <BaseForm
            contract={contract}
            updateContract={updateContract}
            contractName={contract => `from ${contract.port.toString()}/${contract.protocol}` + (contract.container && ` in container ${contract.container}`)}
        >
            <label className={"inline-block w-48"}>
                Destination port:
            </label>
            <input
                value={port}
                onChange={e => {
                    updatePort(e.target.value);
                }}
            />
        </BaseForm>
    );
}