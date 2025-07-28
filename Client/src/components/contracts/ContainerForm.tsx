import {useEffect, useState} from "react";
import {Container} from "@/api/schemas";

type Props = {
    contract: Container;
    variants: Container[];
    updateContract: (contract: Container, isRefetch?: boolean) => void;
}

function VariantName(contract: Container): string {
    return contract.handler?.applicationName ?? "";
}

export default function ContainerForm({contract, variants, updateContract}: Props) {
    const [selected, setSelected] = useState<number>(0);
    useEffect(() => {
        setSelected(0);
    }, [variants]);

    if(variants.length === 0)
    {
        return <></>;
    }
    
    return (
        <div className="card">
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Container
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