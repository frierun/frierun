import {HttpEndpoint} from "@/api/schemas";
import {Contract} from "@/components/contracts/ContractForm.tsx";
import {useEffect, useState} from "react";


type Props = {
    contract: HttpEndpoint;
    variants: HttpEndpoint[];
    allContracts: Contract[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
}

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

export default function HttpEndpointForm({contract, variants, updateContract, allContracts}: Props) {
    const [selected, setSelected] = useState<number>(0);

    useEffect(() => {
        setSelected(0);
    }, [variants]);

    const updateSelected = (idx: number) => {
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
        setSelected(idx);
        updateContract(variants[idx], true);
    }

    return (
        <div className="card">
            <div className="my-1.5">
                <label className={"inline-block w-48"}>Http endpoint to port </label>{contract.port}
                {contract.container && ` in container ${contract.container}`}
            </div>
            <fieldset className="flex gap-4">
                {variants.map((variant, idx) => (
                    <label key={idx} className="flex items-center gap-2">
                        <input
                            type="radio"
                            value={idx}
                            checked={idx === selected}
                            onChange={() => {
                                updateSelected(idx);
                            }}
                        />
                        {VariantName(variant)}
                    </label>
                ))}
            </fieldset>
        </div>
    );
}