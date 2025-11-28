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
     contractRef,
     contract,
     variants,
     updateContract
 }: ContractProps<HttpEndpoint>) {
    const [host, setHost] = useState<string>(contract.resultHost ?? '');

    useEffect(() => {
        setHost(contract.resultHost ?? '');
    }, [contract]);

    return (
        <BaseForm
            contractRef={contractRef}
            contract={contract}
            variants={variants}
            updateContract={updateContract}
            contractName={contract => 'from ' + contract.port.toString() + (contract.container && ` in container ${contract.container}`)}
            variantName={VariantName}
            updateVariant={() => {
                // reset other related contracts
                if (contract.handler?.typeName === 'TraefikHttpEndpointHandler') {
                    updateContract(contractRef.replace("HttpEndpoint:", "Domain:"), null);
                }
                if (contract.handler?.typeName === 'PortHttpEndpointHandler') {
                    updateContract(contractRef.replace('HttpEndpoint', 'PortEndpoint'), null);
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
                            updateContract(contractRef, {
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