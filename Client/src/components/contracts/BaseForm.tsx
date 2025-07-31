import {ReactNode, useEffect, useState} from "react";
import {Contract} from "@/components/contracts/ContractForm.tsx";

type Props<TContract extends Contract> = {
    contract: TContract;
    variants?: TContract[];
    updateContract: (contract: Contract, isRefetch?: boolean) => void;
    contractName?: (contract: TContract) => string;
    variantName?: (contract: TContract) => string;
    updateVariant?: () => void;
    children?: ReactNode | undefined;
}

export default function BaseForm<T extends Contract>
({
     contract,
     variants = [contract],
     updateContract,
     contractName,
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
                {contractName ? contractName(contract) : contract.name}
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
                                    updateContract(variant, true);
                                    if (updateVariant) {
                                        updateVariant();
                                    }
                                }}
                            />
                            {variantName ? variantName(variant) : contract.handler?.applicationName ?? 'Unknown'}
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