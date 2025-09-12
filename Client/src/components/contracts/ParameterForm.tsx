import {useEffect, useState} from "react";
import {Parameter} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function ParameterForm({contractId, contract, updateContract}: ContractProps<Parameter>) {
    const [value, setValue] = useState(contract.defaultValue ?? '');

    useEffect(() => {
        setValue(contract.defaultValue ?? '');
    }, [contract]);

    const updateValue = (value: string) => {
        setValue(value);
        updateContract(contractId, {
            ...contract,
            value: value
        });
    }

    return (
        <BaseForm
            contractId={contractId}
            contract={contract}
            updateContract={updateContract}
        >
            <label className={"inline-block w-48"}>
                Value:
            </label>
            <input
                value={value}
                onChange={e => {
                    updateValue(e.target.value);
                }}
            />
        </BaseForm>
    );
}