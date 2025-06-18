import {Contract} from "@/components/contracts/ContractForm.tsx";
import {useEffect, useState} from "react";
import {Domain} from "@/api/schemas";

type Props = {
    contract: Domain;
    variants: Domain[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
}

function VariantName(contract: Domain): string {
    return contract.handler?.applicationName ?? 'Unknown';
}

export default function DomainForm({contract, variants, updateContract}: Props) {
    const [subdomain, setSubdomain] = useState<string>('');
    
    const domainName = contract.value?.split('.').slice(1).join('.') ?? '';
    const [selected, setSelected] = useState<number>(0);
    
    useEffect(() => {
        setSubdomain(contract.value?.split('.')[0] ?? '');
    }, [contract]);
    
    useEffect(() => {
        setSelected(0);
    }, [variants]);

    return (
        <div className="card">
            <div className="my-1.5">
                <label className={"inline-block w-48"}>Domain</label>
                {contract.name}
            </div>
            <fieldset className="flex gap-4">
                {variants.map((variant, idx) => (
                    <label key={idx} className="flex items-center gap-2">
                        <input
                            type="radio"
                            value={idx}
                            checked={idx === selected}
                            onChange={() => {
                                setSelected(idx);
                                updateContract(variant, true);
                            }}
                        />
                        {VariantName(variant)}
                    </label>
                ))}
            </fieldset>
            <div>
                <label className={"inline-block w-48"}>
                    Domain:
                </label>
                <input
                    value={subdomain}
                    onChange={e => {
                        setSubdomain(e.target.value);
                        updateContract({
                            ...contract,
                            value: `${e.target.value}.${domainName}`
                        });
                    }}
                />
                <span className="ml-2">.{domainName}</span>
            </div>
        </div>
    );
}