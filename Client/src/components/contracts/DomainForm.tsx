import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import {useEffect, useState} from "react";
import {Domain} from "@/api/schemas";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function DomainForm({contractRef, contract, variants, updateContract}: ContractProps<Domain>) {
    const [subdomain, setSubdomain] = useState<string>('');
    const domainName = contract.value?.split('.').slice(1).join('.') ?? '';

    useEffect(() => {
        setSubdomain(contract.value?.split('.')[0] ?? '');
    }, [contract]);

    return (
        <BaseForm
            contractRef={contractRef}
            contract={contract}
            variants={variants}
            updateContract={updateContract}
        >
            <div>
                <label className={"inline-block w-48"}>
                    Domain:
                </label>
                <input
                    value={subdomain} onChange={e => {
                    setSubdomain(e.target.value);
                    updateContract(contractRef, {
                        ...contract,
                        value: `${e.target.value}.${domainName}`
                    });
                }}
                />
                <span className="ml-2">.{domainName}</span>
            </div>
        </BaseForm>
    );
}