import {Optional} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function OptionalForm({contractId, contract, variants, updateContract}: ContractProps<Optional>) {
    return (
        <BaseForm
            contractId={contractId}
            contract={contract}
            variants={variants}
            updateContract={updateContract}
            variantName={contract => contract.value ? "Enable" : "Disable"}
        />        
    );
}