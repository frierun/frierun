import {ReactNode, useEffect, useState} from "react";
import {Contract} from "@/types.ts";

type Props<TContract extends Contract> = {
    contractId: string;
    contract: TContract;
    variants?: TContract[];
    updateContract: (contractId: string, contract: Contract, isRefetch?: boolean) => void;
    contractName?: (contractId: string, contract: TContract) => string;
    variantName?: (contract: TContract) => string;
    updateVariant?: () => void;
    children?: ReactNode | undefined;
}

export default function BaseForm<T extends Contract>
({
     contractId,
     contract,
     variants = [contract],
     updateContract,
     contractName = contractId => contractId,
     variantName = contract => contract.handler?.applicationName ?? 'Unknown',
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
                {contractName(contractId, contract)}
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
                                    updateContract(contractId, variant, true);
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