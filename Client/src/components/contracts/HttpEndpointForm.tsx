import {HttpEndpoint} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import {useEffect, useState} from "react";
import BaseForm from "@/components/contracts/BaseForm.tsx";

function VariantName(contract: HttpEndpoint): string {
    switch (contract.handler?.typeName) {
        case 'TraefikHttpEndpointHandler':
            return `Traefik`;
        case 'CloudflareHttpEndpointHandler':
            return `Cloudflare`;
        case 'PortHttpEndpointHandler':
            return `Port`;
        default:
            return 'Unknown';
    }
}

export default function HttpEndpointForm
({
     contract,
     variants,
     updateContract,
     allContracts
 }: ContractProps<HttpEndpoint>) {
    const [host, setHost] = useState<string>(contract.resultHost ?? '');

    useEffect(() => {
        setHost(contract.resultHost ?? '');
    }, [contract]);

    return (
        <BaseForm
            contract={contract}
            variants={variants}
            updateContract={updateContract}
            contractName={contract => contract.port.toString() + (contract.container && ` in container ${contract.container}`)}
            variantName={VariantName}
            updateVariant={() => {
                // reset other related contracts
                if (contract.handler?.typeName === 'TraefikHttpEndpointHandler') {
                    const domainContract = allContracts.find(c => c.type === 'Domain' && c.name === contract.domain);
                    if (domainContract) {
                        updateContract(domainContract);
                    }
                }
                if (contract.handler?.typeName === 'PortHttpEndpointHandler') {
                    const portContract = allContracts.find(c => c.type === 'PortEndpoint' && c.port === contract.port && c.protocol === 'Tcp');
                    if (portContract) {
                        updateContract(portContract);
                    }
                }
            }}
        >
            {contract.handler?.typeName === 'CloudflareHttpEndpointHandler' && (
                <div className="my-1.5">
                    <label className={"inline-block w-48"}>Target host:</label>
                    <input
                        value={host}
                        onChange={e => {
                            setHost(e.target.value);
                            updateContract({
                                ...contract,
                                resultHost: e.target.value
                            });
                        }}
                    />
                </div>
            )}
        </BaseForm>
    );
}