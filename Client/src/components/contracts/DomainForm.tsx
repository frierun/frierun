import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import {useState} from "react";
import {Domain} from "@/api/schemas";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function DomainForm({contract, variants, updateContract}: ContractProps<Domain>) {
    const [subdomain, setSubdomain] = useState<string>('');
    const domainName = contract.value?.split('.').slice(1).join('.') ?? '';

    return (
        <BaseForm
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
                        updateContract({
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