import {Optional} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function OptionalForm({contract, variants, updateContract}: ContractProps<Optional>) {
    return (
        <BaseForm
            contract={contract}
            variants={variants}
            updateContract={updateContract}
            variantName={contract => contract.value ? "Enable" : "Disable"}
        />        
    );
}