import {useEffect, useState} from "react";
import {Optional} from "@/api/schemas";

type Props = {
    contract: Optional;
    variants: Optional[];
    updateContract: (contract: Optional, isRefetch?: boolean) => void;
}

function VariantName(contract: Optional): string {
    return contract.value ? "Enable" : "Disable";
}

export default function OptionalForm({contract, variants, updateContract}: Props) {
    const [selected, setSelected] = useState<number>(0);
    useEffect(() => {
        setSelected(0);
    }, [variants]);

    return (
        <div className="card">
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Option
                </label>
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
        </div>
    );
}