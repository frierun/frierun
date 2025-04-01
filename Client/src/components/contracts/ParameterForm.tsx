import {useEffect, useState} from "react";
import {Parameter} from "@/api/schemas";

type Props = {
    contract: Parameter;
    updateContract: (contract: Parameter) => void;
}

export default function ParameterForm({contract, updateContract}: Props) {
    const [value, setValue] = useState(contract.defaultValue ?? '');

    useEffect(() => {
        setValue(contract.defaultValue ?? '');
    }, [contract]);

    const updateValue = (value: string) => {
        setValue(value);
        updateContract({
            ...contract,
            value: value
        });
    }

    return (
        <>
            <div>
                <div className={"my-1.5"}>
                    <label className={"inline-block w-48"}>
                        Parameter {contract.name}
                    </label>
                </div>
                <label className={"inline-block w-48"}>
                    Value:
                </label>
                <input
                    value={value}
                    onChange={e => {
                        updateValue(e.target.value);
                    }}
                />
            </div>
        </>
    );
}