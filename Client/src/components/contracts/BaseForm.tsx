import {ReactNode, useEffect, useState} from "react";
import {Contract} from "@/types.ts";

type Props<TContract extends Contract> = {
    contractRef: string;
    contract: TContract;
    variants?: TContract[];
    updateContract: (contractRef: string, contract: Contract, isRefetch?: boolean) => void;
    contractName?: (contract: TContract) => string;
    variantName?: (contract: TContract) => string;
    updateVariant?: () => void;
    children?: ReactNode | undefined;
}

export default function BaseForm<T extends Contract>
({
     contractRef,
     contract,
     variants = [contract],
     updateContract,
     contractName,
     variantName = contract => contract.id ?? contract.handler?.applicationName ?? 'New',
     updateVariant,
     children
 }: Props<T>) {
    const [selected, setSelected] = useState<number>(0);
    useEffect(() => {
        setSelected(0);
    }, [variants]);

    if (variants.length === 0) {
        return <></>;
    }

    if (variants.length === 1 && !children) {
        return <></>;
    }

    return (
        <div className="card">
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    {contract.type}
                </label>
                {contractName ? contractName(contract) : contractRef.split(':')[1]}
            </div>
            {variants.length > 1 && (
                <fieldset className="flex gap-4">
                    {variants.map((variant, idx) => (
                        <label key={idx} className="flex items-center gap-2">
                            <input
                                type="radio"
                                value={idx}
                                checked={idx === selected}
                                onChange={() => {
                                    setSelected(idx);
                                    updateContract(contractRef, variant, true);
                                    if (updateVariant) {
                                        updateVariant();
                                    }
                                }}
                            />
                            {variantName(variant)}
                        </label>
                    ))}
                </fieldset>
            )}
            {children && (
                <div className="my-1.5">{children}</div>
            )}
        </div>
    );
}