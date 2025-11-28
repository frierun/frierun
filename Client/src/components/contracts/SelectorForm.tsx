import {Selector} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function SelectorForm({contractRef, contract, variants, updateContract}: ContractProps<Selector>) {
    return (
        <BaseForm
            contractRef={contractRef}
            contract={contract}
            variants={variants}
            variantName={contract => contract.value ?? ""}
            updateContract={updateContract}
        />
    );
}