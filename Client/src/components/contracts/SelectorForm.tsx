import {useEffect, useState} from "react";
import {Selector} from "@/api/schemas";

type Props = {
    contract: Selector;
    updateContract: (contract: Selector) => void;
}

export default function SelectorForm({contract, updateContract}: Props) {
    const [value, setValue] = useState(contract.value ?? '');

    useEffect(() => {
        setValue(contract.value ?? '');
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
                        Selector {contract.name}
                    </label>
                </div>
                <fieldset className={"flex gap-4"}>
                    {contract.options.map(option => (
                        <div key={option.name}>
                            <input
                                type="radio"
                                id={`Selector${contract.name}${option.name}Radio`}
                                value={option.name}
                                checked={value === option.name}
                                onChange={() => { updateValue(option.name); }}
                            >
                            </input>
                            <label htmlFor={`Selector${contract.name}${option.name}Radio`}>
                                {option.name}
                            </label>
                        </div>
                    ))}
                </fieldset>
            </div>
        </>
    );
}